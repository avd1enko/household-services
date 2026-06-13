using HouseholdServices.Application.DTOs.MasterProfile;
using HouseholdServices.Application.DTOs.ServiceCategories;

namespace HouseholdServices.Application.Services.MasterProfiles;

public interface IMasterProfileService
{
    Task<MasterProfileResponse> GetCurrentAsync();
    Task UpdateCurrentAsync(UpdateMasterProfileRequest request);
    Task<IReadOnlyCollection<ServiceCategoryResponse>> GetCurrentCategoriesAsync();
    Task ReplaceCurrentCategoriesAsync(UpdateMasterCategoriesRequest request);
}