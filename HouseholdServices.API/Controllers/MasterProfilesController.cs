using HouseholdServices.Application.DTOs.MasterProfile;
using HouseholdServices.Application.Services.MasterProfiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HouseholdServices.Application.DTOs.ServiceCategories;
using HouseholdServices.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HouseholdServices.API.Controllers;

[ApiController]
[Route("api/masters/me")]
[Authorize]
public class MasterProfilesController : ControllerBase
{
    private readonly IMasterProfileService _masterProfileService;
    private readonly HouseholdServicesDbContext _dbContext;

    public MasterProfilesController(IMasterProfileService masterProfileService, HouseholdServicesDbContext dbContext)
    {
        _masterProfileService = masterProfileService;
        _dbContext = dbContext;
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
    
    // абсолютный роут, игнорирующий установленный общий
    [HttpGet("~/api/service-categories")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyCollection<ServiceCategoryResponse>>> GetServiceCategoriesAsync()
    {
        List<ServiceCategoryResponse> categories = await _dbContext.ServiceCategories
            .AsNoTracking()
            .OrderBy(category => category.CategoryId)
            .Select(category => new ServiceCategoryResponse
            {
                CategoryId = category.CategoryId,
                Name = category.Name
            })
            .ToListAsync();

        return Ok(categories);
    }
}