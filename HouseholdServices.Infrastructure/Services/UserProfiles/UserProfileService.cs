using HouseholdServices.Application.DTOs.UserProfile;
using HouseholdServices.Application.Services.UserProfiles;
using HouseholdServices.Application.Services.Users;
using HouseholdServices.Domain.Entities;
using HouseholdServices.Infrastructure.Data;
using HouseholdServices.Infrastructure.Services.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HouseholdServices.Infrastructure.Services.UserProfiles;

public class UserProfileService : IUserProfileService
{
    private readonly HouseholdServicesDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPasswordHasher<User> _passwordHasher;
    
    public UserProfileService(HouseholdServicesDbContext dbContext, ICurrentUserService currentUserService, IPasswordHasher<User> passwordHasher)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _passwordHasher = passwordHasher;
    }
    
    public async Task<UserProfileResponse> GetUserProfileAsync()
    {
        int currentUserId = _currentUserService.GetUserId();
        string role = _currentUserService.GetRole();

        User? user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.UserId == currentUserId);
        
        if (user == null)
            throw new InvalidOperationException("User not found");
        
        int completedStatusId = await _dbContext.OrderStatuses
            .Where(status => status.Name == "completed")
            .Select(status => status.OrderStatusId)
            .FirstOrDefaultAsync();
        int completedOrdersCount;

        if (role == "client")
        {
            completedOrdersCount = await _dbContext.Orders
                .AsNoTracking()
                .Where(order =>
                    order.OrderStatusId == completedStatusId && 
                        order.Response.Request.ClientId == currentUserId)
                .CountAsync();
        }
        else if (role == "master")
        {
            completedOrdersCount = await _dbContext.Orders
                .AsNoTracking()
                .Where(order =>
                    order.OrderStatusId == completedStatusId && 
                    order.Response.MasterId == currentUserId)
                .CountAsync();
        }
        else
        {
            completedOrdersCount = 0;
        }
        return new UserProfileResponse

        {
            UserId = user.UserId,
            Login = user.Login,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Phone = user.Phone,
            Role = role,
            CreatedAt = user.CreatedAt,
            DaysSinceRegistration = (DateTime.UtcNow.Date - user.CreatedAt.Date).Days,
            CompletedOrdersCount = completedOrdersCount
        };
    }

    public async Task UpdateCurrentUserAsync(UpdateUserProfileRequest request)
    {
        int currentUserId = _currentUserService.GetUserId();

        
        User? user = await _dbContext.Users
            .FirstOrDefaultAsync(user => user.UserId == currentUserId);
        
        if (user == null)
            throw new InvalidOperationException("User not found");
        
        if (request.FirstName != null)
            user.FirstName = request.FirstName;
        if (request.LastName != null)
            user.LastName = request.LastName;
        if (request.Phone != null)
            user.Phone = request.Phone;
        await _dbContext.SaveChangesAsync();
        
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest request)
    {
        int currentUserId = _currentUserService.GetUserId();
        User? user = await _dbContext.Users
            .FirstOrDefaultAsync(user => user.UserId == currentUserId);
        
        if (user == null)
            throw new InvalidOperationException("User not found");
        
        PasswordVerificationResult passwordResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.CurrentPassword);
        
        if (passwordResult == PasswordVerificationResult.Failed)
            throw new InvalidOperationException("Current password is incorrect.");
        
        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        await _dbContext.SaveChangesAsync();
    }
}