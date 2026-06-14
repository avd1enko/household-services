using HouseholdServices.Application.DTOs.UserProfile;

namespace HouseholdServices.Application.Services.UserProfiles;

public interface IUserProfileService
{
    Task<UserProfileResponse> GetUserProfileAsync();
    Task UpdateCurrentUserAsync(UpdateUserProfileRequest request);
    Task ChangePasswordAsync(ChangePasswordRequest request);

}