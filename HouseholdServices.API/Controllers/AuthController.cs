using HouseholdServices.Application.DTOs.Auth;
using HouseholdServices.Application.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HouseholdServices.Application.Services.Users;
using HouseholdServices.Infrastructure.Services.Users;

namespace HouseholdServices.API.Controllers;

// объявляем коду, что это контроллер, который будет обрабатывать http запросы
[ApiController]
// базовый путь для контроллеров внутри
[Route("api/auth")]
public class AuthController : ControllerBase // наследуемся от базового класса апи контроллеров из mvc
{
    private readonly IAuthService _authService; // контроллер зависит не от конкретной реализации, а от контракта
    private readonly ICurrentUserService _currentUserService;

    public AuthController(IAuthService authService, ICurrentUserService currentUserService)
    {
        _authService = authService; // dependency injection (создаем AuthService и передаем его в конструктор)
        _currentUserService = currentUserService;
    }

    [HttpPost("register")] // POST route/register
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request) // указываем dtos
    {
        AuthResponse response = await _authService.RegisterAsync(request); // вызываем метод регистрации из сервиса
        
        return Ok(response); 
    }
    
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        AuthResponse response = await _authService.LoginAsync(request);
        
        return Ok(response);
    }
    // временный эндпоинт
    [Authorize]
    [HttpGet("protected-test")]
    public IActionResult ProtectedTest()
    {
        return Ok("You are authorized");
    }
    
    [Authorize]
    [HttpGet("current-user-test")]
    public IActionResult CurrentUserTest()
    {
        int userId = _currentUserService.GetUserId();
        string role = _currentUserService.GetRole();

        return Ok(new
        {
            UserId = userId,
            Role = role
        });
    }
}