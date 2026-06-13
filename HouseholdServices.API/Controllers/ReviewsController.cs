using HouseholdServices.Application.DTOs.Review;
using HouseholdServices.Application.Exceptions.Review;
using HouseholdServices.Application.Services.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseholdServices.API.Controllers;

[ApiController]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [Authorize(Roles = "client")]
    [HttpPost("api/orders/{orderId:int}/reviews")]
    public async Task<ActionResult<ReviewResponse>> Create(int orderId, CreateReviewRequest request)
    {
        try
        {
            ReviewResponse response = await _reviewService.CreateAsync(orderId, request);
            return Created($"api/orders/{orderId}/reviews", response);
        }
        catch (InvalidReviewRatingException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (ReviewOrderNotFoundException exception)
        {
            return NotFound(exception.Message);
        }
        catch (ReviewAccessDeniedException)
        {
            return Forbid();
        }
        catch (ReviewOrderNotCompletedException exception)
        {
            return Conflict(exception.Message);
        }
        catch (ReviewAlreadyExistsException exception)
        {
            return Conflict(exception.Message);
        }
    }

    [HttpGet("api/masters/{masterId:int}/reviews")]
    public async Task<ActionResult<IReadOnlyCollection<MasterReviewListItemResponse>>> GetByMasterId(int masterId)
    {
        IReadOnlyCollection<MasterReviewListItemResponse> response = await _reviewService.GetByMasterIdAsync(masterId);
        return Ok(response);
    }
}
