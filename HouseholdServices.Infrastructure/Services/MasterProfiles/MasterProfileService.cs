using HouseholdServices.Application.DTOs.MasterProfile;
using HouseholdServices.Application.DTOs.ServiceCategories;
using HouseholdServices.Application.Services.MasterProfiles;
using HouseholdServices.Application.Services.Users;
using HouseholdServices.Domain.Entities;
using HouseholdServices.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HouseholdServices.Infrastructure.Services.MasterProfiles;

public class MasterProfileService : IMasterProfileService
{
    private readonly HouseholdServicesDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public MasterProfileService(HouseholdServicesDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<MasterProfileResponse> GetCurrentAsync()
    {
        string currentUserRole = _currentUserService.GetRole();

        if (currentUserRole != "master")
            throw new UnauthorizedAccessException();

        int currentUserId = _currentUserService.GetUserId();

        MasterProfile? masterProfile = await _dbContext.MasterProfiles
            .AsNoTracking()
            .Include(masterProfile => masterProfile.User)
            .FirstOrDefaultAsync(masterProfile => masterProfile.UserId == currentUserId);

        if (masterProfile == null)
            throw new InvalidOperationException("Master profile not found.");

        List<ServiceCategoryResponse> categories = await _dbContext.MasterCategories
            .AsNoTracking()
            .Where(masterCategory => masterCategory.UserId == currentUserId)
            .Select(masterCategory => new ServiceCategoryResponse
            {
                CategoryId = masterCategory.Category.CategoryId,
                Name = masterCategory.Category.Name
            })
            .ToListAsync();

        return new MasterProfileResponse
        {
            UserId = masterProfile.User.UserId,
            Login = masterProfile.User.Login,
            FirstName = masterProfile.User.FirstName,
            LastName = masterProfile.User.LastName,
            Phone = masterProfile.User.Phone,
            Description = masterProfile.Description,
            ExperienceYears = masterProfile.ExperienceYears,
            Categories = categories
        };
    }

    public async Task UpdateCurrentAsync(UpdateMasterProfileRequest request)
    {
        string currentUserRole = _currentUserService.GetRole();
        if (currentUserRole != "master")
            throw new UnauthorizedAccessException();

        int currentUserId = _currentUserService.GetUserId();
        MasterProfile? masterProfile = await _dbContext.MasterProfiles
            .FirstOrDefaultAsync(masterProfile => masterProfile.UserId == currentUserId);

        if (masterProfile == null)
            throw new InvalidOperationException("Master profile not found.");
        if (request.Description != null)
            masterProfile.Description = request.Description;

        if (request.ExperienceYears != null)
        {
            if (request.ExperienceYears < 0)
                throw new InvalidOperationException("Experience years cannot be negative.");
            masterProfile.ExperienceYears = request.ExperienceYears.Value;
        }
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<ServiceCategoryResponse>> GetCurrentCategoriesAsync()
    {
        string currentUserRole = _currentUserService.GetRole();

        if (currentUserRole != "master")
            throw new UnauthorizedAccessException();

        int currentUserId = _currentUserService.GetUserId();

        List<ServiceCategoryResponse> categories = await _dbContext.MasterCategories
            .AsNoTracking()
            .Where(masterCategory => masterCategory.UserId == currentUserId)
            .Select(masterCategory => new ServiceCategoryResponse
            {
                CategoryId = masterCategory.Category.CategoryId,
                Name = masterCategory.Category.Name
            })
            .ToListAsync();

        return categories;
    }

    public async Task ReplaceCurrentCategoriesAsync(UpdateMasterCategoriesRequest request)
    {
        string currentUserRole = _currentUserService.GetRole();
        if (currentUserRole != "master")
            throw new UnauthorizedAccessException();
        
        int currentUserId = _currentUserService.GetUserId();
        bool masterProfileExists = await _dbContext.MasterProfiles
            .AnyAsync(masterProfile => masterProfile.UserId == currentUserId);

        if (!masterProfileExists)
            throw new InvalidOperationException("Master profile not found.");
        bool hasDuplicates = request.CategoryIds
            .Distinct()
            .Count() != request.CategoryIds.Count;

        if (hasDuplicates)
            throw new InvalidOperationException("Duplicate categories are not allowed.");
        int existingCategoriesCount = await _dbContext.ServiceCategories
            .CountAsync(category => request.CategoryIds.Contains(category.CategoryId));
        if (existingCategoriesCount != request.CategoryIds.Count)
            throw new InvalidOperationException("One or more service categories do not exist.");

        List<MasterCategory> currentMasterCategories = await _dbContext.MasterCategories
            .Where(masterCategory => masterCategory.UserId == currentUserId)
            .ToListAsync();
        _dbContext.MasterCategories.RemoveRange(currentMasterCategories);

        List<MasterCategory> newMasterCategories = request.CategoryIds
            .Select(categoryId => new MasterCategory
            {
                UserId = currentUserId,
                CategoryId = categoryId
            })
            .ToList();
        _dbContext.MasterCategories.AddRange(newMasterCategories);
        await _dbContext.SaveChangesAsync();
    }
}