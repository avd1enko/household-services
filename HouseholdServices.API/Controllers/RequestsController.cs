using HouseholdServices.Application.DTOs.Request;
using HouseholdServices.Application.Exceptions.Request;
using HouseholdServices.Application.Services.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseholdServices.API.Controllers;

[ApiController]
[Route("api/requests")]
public class RequestsController : ControllerBase
{
    private readonly IRequestService _requestService;

    public RequestsController(IRequestService requestService)
    {
        _requestService = requestService;
    }

    [Authorize(Roles = "master")]
    [HttpGet]
    public async Task<ActionResult<List<AvailableRequestListItemResponse>>> GetAvailableForMaster(
        [FromQuery] RequestFilterRequest filter)
    {
        List<AvailableRequestListItemResponse> response =
            await _requestService.GetAvailableForCurrentMasterAsync(filter);
        return Ok(response);
    }

    [Authorize(Roles = "client")]
    [HttpGet("/api/users/me/requests")]
    public async Task<ActionResult<List<UserRequestListItemResponse>>> GetCurrentUserRequests(
        [FromQuery] RequestFilterRequest filter)
    {
        List<UserRequestListItemResponse> response = await _requestService.GetCurrentUserRequestsAsync(filter);
        return Ok(response);
    }

    [Authorize(Roles = "client,master")]
    [HttpGet("{requestId:int}")]
    public async Task<ActionResult<RequestResponse>> GetById(int requestId)
    {
        try
        {
            RequestResponse response = await _requestService.GetByIdAsync(requestId);
            return Ok(response);
        }
        catch (RequestNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (RequestAccessDeniedException exception)
        {
            return NotFound(exception.Message);
        }
    }

    [Authorize(Roles = "client")]
    [HttpPost]
    public async Task<ActionResult<RequestResponse>> Create(CreateRequestRequest request)
    {
        try
        {
            RequestResponse response = await _requestService.CreateAsync(request);
            return Created(string.Empty, response);
        }
        catch (CategoryNotFoundException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (ClientRoleRequiredException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, exception.Message);
        }
    }

    [Authorize(Roles = "client")]
    [HttpPatch("{requestId:int}/cancel")]
    public async Task<IActionResult> Cancel(int requestId)
    {
        try
        {
            await _requestService.CancelAsync(requestId);
            return NoContent();
        }
        catch (RequestNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (RequestAccessDeniedException exception)
        {
            return NotFound(exception.Message);
        }
        catch (RequestCannotBeCancelledException exception)
        {
            return Conflict(exception.Message);
        }
    }
}
