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

    [Authorize(Roles = "client,master")]
    [HttpGet]
    public async Task<ActionResult<List<RequestResponse>>> GetAll([FromQuery] RequestFilterRequest filter)
    {
        List<RequestResponse> response = await _requestService.GetAllAsync(filter);
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
            return StatusCode(StatusCodes.Status403Forbidden, exception.Message);
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
}
