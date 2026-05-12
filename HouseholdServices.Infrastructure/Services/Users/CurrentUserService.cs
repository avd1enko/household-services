using HouseholdServices.Application.Services.Users;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace HouseholdServices.Infrastructure.Services.Users;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int GetUserId()
    {
        string? userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier); // значение того самого claim для юзерАйди

        if (userId is null)
            throw new InvalidOperationException("User id claim does not exist");
        return int.Parse(userId);
    }

    public string GetRole()
    {
        string? role = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
        if (role is null)
            throw new InvalidOperationException("User role claim does not exist");

        return role;
    }
}