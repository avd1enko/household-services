using HouseholdServices.Application.DTOs.Auth;
using HouseholdServices.Application.Services.Auth;
using HouseholdServices.Infrastructure.Data;
using HouseholdServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity; // для хеширования
using HouseholdServices.Application.Exceptions.Auth;

namespace HouseholdServices.Infrastructure.Services.Auth;

public class AuthService : IAuthService
{
    private readonly HouseholdServicesDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(HouseholdServicesDbContext dbContext, IPasswordHasher<User> passwordHasher, IJwtTokenService jwtTokenService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
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
            throw new LoginAlreadyTakenException();

        Role? role =
            await _dbContext.Roles.FirstOrDefaultAsync(role =>
                role.Name == request.Role); // при помощи встроенной функции вытаскиваем и сам объект типа роль

        // FirstOrDefault      → обычный синхронный метод, await нельзя (БЛОКИРУЕТ ТЕКУЩИЙ ПОТОК КОДА, ПОКА БД НЕ ОТВЕТИТ)
        // FirstOrDefaultAsync → асинхронный метод EF Core, await можно (НЕ БЛОКИРУЕТ)
        if (role is null)
            throw new RoleNotFoundException();

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
        

        // теперь создаем запись в связующей таблице UserRole
        UserRole userRole = new UserRole
        {
            User = user,
            RoleId = role.RoleId
        };
        _dbContext.UserRoles.Add(userRole);
        await _dbContext.SaveChangesAsync();
        
        return new AuthResponse
        {
            UserId = user.UserId,
            Login = user.Login,
            Role = role.Name,
            Token = _jwtTokenService.GenerateJwtToken(user, role.Name)
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        User? user = await _dbContext.Users.FirstOrDefaultAsync(user => user.Login == request.Login);

        if (user is null)
            throw new InvalidCredentialsException();
        
        // встроенный метод для проверки хешированного пароля
        PasswordVerificationResult passwordResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (passwordResult == PasswordVerificationResult.Failed)
            throw new InvalidCredentialsException();
        
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
            Token = _jwtTokenService.GenerateJwtToken(user, userRole.Role.Name)
        };
    }
}
