using HouseholdServices.Application.DTOs.UserProfile;
using HouseholdServices.Application.Services.UserProfiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseholdServices.API.Controllers;

[ApiController]
[Route("api/users/me")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserProfileService _userProfileService;

    public UsersController(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    [HttpGet]
    public async Task<ActionResult<UserProfileResponse>> GetCurrentAsync()
    {
        UserProfileResponse response = await _userProfileService.GetUserProfileAsync();

        return Ok(response);
    }

    [HttpPatch]
    public async Task<IActionResult> UpdateCurrentAsync(UpdateUserProfileRequest request)
    {
        await _userProfileService.UpdateCurrentUserAsync(request);

        return NoContent();
    }

    [HttpPatch("password")]
    public async Task<IActionResult> ChangePasswordAsync(ChangePasswordRequest request)
    {
        await _userProfileService.ChangePasswordAsync(request);

        return NoContent();
    }
}