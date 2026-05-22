using HouseholdServices.Application.DTOs.Response;
using HouseholdServices.Application.Exceptions.Response;
using HouseholdServices.Application.Services.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseholdServices.API.Controllers;

[ApiController]
[Authorize]
public class ResponsesController : ControllerBase
{
    private readonly IResponseService _responseService;

    public ResponsesController(IResponseService responseService)
    {
        _responseService = responseService;
    }

    [Authorize(Roles = "master")]
    [HttpPost("api/requests/{requestId:int}/responses")]
    public async Task<ActionResult<ResponseForRequestListItemResponse>> Create(int requestId, CreateResponseRequest request)
    {
        try
        {
            ResponseForRequestListItemResponse response = await _responseService.CreateAsync(requestId, request);
            return Created($"api/requests/{requestId}/responses", response);
        }
        catch (InvalidResponsePriceException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (RequestNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (RequestNotActiveException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (ResponseAlreadyExistsException exception)
        {
            return Conflict(exception.Message);
        }
        catch (ResponseStatusNotFoundException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("api/requests/{requestId:int}/responses")]
    public async Task<ActionResult<IReadOnlyCollection<ResponseForRequestListItemResponse>>> GetByRequestId(int requestId)
    {
        try
        {
            IReadOnlyCollection<ResponseForRequestListItemResponse> responses = await _responseService.GetByRequestIdAsync(requestId);
            return Ok(responses);
        }
        catch (RequestNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
    }

    [Authorize(Roles = "master")]
    [HttpGet("api/masters/me/responses")]
    public async Task<ActionResult<IReadOnlyCollection<MasterResponseListItemResponse>>> GetCurrentMasterResponses()
    {
        IReadOnlyCollection<MasterResponseListItemResponse> responses = await _responseService.GetCurrentMasterResponsesAsync();
        return Ok(responses);
    }
}
