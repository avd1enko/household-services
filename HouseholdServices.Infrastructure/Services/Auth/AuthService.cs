using HouseholdServices.Application.DTOs.Auth;
using HouseholdServices.Application.Services.Auth;
using HouseholdServices.Infrastructure.Data;
using HouseholdServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity; // для хеширования

namespace HouseholdServices.Infrastructure.Services.Auth;

public class AuthService : IAuthService
{
    private readonly HouseholdServicesDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthService(HouseholdServicesDbContext dbContext, IPasswordHasher<User> passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {

        bool loginExists =
            await _dbContext.Users.AnyAsync(q =>
                q.Login == request
                    .Login); // используем лямбда функцию где user - объект User , а после => условие проверки
        // (то есть найти объект user для которого истино, что его логин равен передаваемому логину)
        // тип user (в этом случае q для наглядности, берется из того, к чему мы применяем AnyAsync (функция EF core)

        if (loginExists)
            throw new InvalidOperationException("This login has already been taken");

        Role? role =
            await _dbContext.Roles.FirstOrDefaultAsync(role =>
                role.Name == request.Role); // при помощи встроенной функции вытаскиваем и сам объект типа роль

        // FirstOrDefault      → обычный синхронный метод, await нельзя (БЛОКИРУЕТ ТЕКУЩИЙ ПОТОК КОДА, ПОКА БД НЕ ОТВЕТИТ)
        // FirstOrDefaultAsync → асинхронный метод EF Core, await можно (НЕ БЛОКИРУЕТ)
        if (role is null)
            throw new InvalidOperationException("This role does not exist");

        User user = new User
        {
            Login = request.Login,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _dbContext.Users.Add(user);

        await _dbContext.SaveChangesAsync();

        // теперь создаем запись в связующей таблице UserRole
        UserRole userRole = new UserRole
        {
            UserId = user.UserId,
            RoleId = role.RoleId
        };
        _dbContext.UserRoles.Add(userRole);
        await _dbContext.SaveChangesAsync();


        // ПОЗЖЕ ЗАМЕНИТЬ НА JWT
        return new AuthResponse
        {
            UserId = user.UserId,
            Login = user.Login,
            Role = role.Name,
            Token = "temporary-token"
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        User? user = await _dbContext.Users.FirstOrDefaultAsync(user => user.Login == request.Login);

        if (user is null)
            throw new InvalidOperationException("Incorrect login or password");
        
        // встроенный метод для проверки хешированного пароля
        PasswordVerificationResult passwordResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (passwordResult == PasswordVerificationResult.Failed)
            throw new InvalidOperationException("Incorrect login or password");
        
        UserRole? userRole = await _dbContext.UserRoles
            .Include(userRole => userRole.Role)
            .FirstOrDefaultAsync(userRole => userRole.UserId == user.UserId);

        if (userRole is null)
        {
            throw new InvalidOperationException("User role does not exist");
        }
        
        return new AuthResponse
        {
            UserId = user.UserId,
            Login = user.Login,
            Role = userRole.Role.Name,
            Token = "temporary-token"
        };
    }
}
/* 1. Найти пользователя по request.Login
2. Если пользователь не найден — ошибка
3. Проверить request.Password против user.PasswordHash
4. Если пароль неверный — ошибка
5. Получить роль пользователя
6. Вернуть AuthResponse
7. Позже заменить temporary-token на JWT */