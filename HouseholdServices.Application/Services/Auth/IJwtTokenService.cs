using HouseholdServices.Domain.Entities;
namespace HouseholdServices.Application.Services.Auth;

public interface IJwtTokenService
{
    string GenerateJwtToken(User user, string role);
}