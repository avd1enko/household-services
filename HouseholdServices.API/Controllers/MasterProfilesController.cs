using HouseholdServices.Application.DTOs.MasterProfile;
using HouseholdServices.Application.Services.MasterProfiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseholdServices.API.Controllers;

[ApiController]
[Route("api/masters/me")]
[Authorize]
public class MasterProfilesController : ControllerBase
{
    private readonly IMasterProfileService _masterProfileService;

    public MasterProfilesController(IMasterProfileService masterProfileService)
    {
        _masterProfileService = masterProfileService;
    }

    [HttpGet]
    public async Task<ActionResult<MasterProfileResponse>> GetCurrentAsync()
    {
        MasterProfileResponse response = await _masterProfileService.GetCurrentAsync();
        return Ok(response);
    }

    [HttpPatch]
    public async Task<IActionResult> UpdateCurrentAsync(UpdateMasterProfileRequest request)
    {
        await _masterProfileService.UpdateCurrentAsync(request);
        return NoContent();
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCurrentCategoriesAsync()
    {
        var response = await _masterProfileService.GetCurrentCategoriesAsync();
        return Ok(response);
    }

    [HttpPut("categories")]
    public async Task<IActionResult> ReplaceCurrentCategoriesAsync(UpdateMasterCategoriesRequest request)
    {
        await _masterProfileService.ReplaceCurrentCategoriesAsync(request);
        return NoContent();
    }
}