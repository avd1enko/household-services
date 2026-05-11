using HouseholdServices.Application.DTOs.Auth;
namespace HouseholdServices.Application.Services.Auth;

public class AuthService: IAuthService
{
 public Task<AuthResponse> RegisterAsync(RegisterRequest request)
 {
  throw new NotImplementedException();
 }
 
 public Task<AuthResponse> LoginAsync(LoginRequest request)
 {
  throw new NotImplementedException();
 }
}