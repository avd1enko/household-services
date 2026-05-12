using HouseholdServices.Application.DTOs.Auth;
using HouseholdServices.Application.Services.Auth;
using Microsoft.AspNetCore.Mvc;
namespace HouseholdServices.API.Controllers;

// объявляем коду, что это контроллер, который будет обрабатывать http запросы
[ApiController]
// базовый путь для контроллеров внутри
[Route("api/auth")]
public class AuthController : ControllerBase // наследуемся от базового класса апи контроллеров из mvc
{
    private readonly IAuthService _authService; // контроллер зависит не от конкретной реализации, а от контракта

    public AuthController(IAuthService authService)
    {
        _authService = authService; // dependency injection (создаем AuthService и передаем его в конструктор)
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
}