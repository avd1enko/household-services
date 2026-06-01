using Microsoft.AspNetCore.Mvc;
using HouseholdServices.Application.DTOs.Order;
using HouseholdServices.Application.Exceptions.Order;
using HouseholdServices.Application.Services.Order;

namespace HouseholdServices.API.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    
    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("{orderId:int}")]
    public async Task<ActionResult<OrderResponse>> GetById(int orderId)
    {
        try
        {
            OrderResponse response = await _orderService.GetByIdAsync(orderId);
            return Ok(response);
        }
        catch (OrderNotFoundException exception)
        {
            return StatusCode(StatusCodes.Status404NotFound, exception.Message);
        }
        catch (OrderAccessDeniedException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, exception.Message);
        }
    }

    [HttpGet("/api/users/me/orders")]
    public async Task<ActionResult<IReadOnlyCollection<UserOrderListItemResponse>>> GetCurrentClientOrders()
    {
        try
        {
            IReadOnlyCollection<UserOrderListItemResponse> response =
                await _orderService.GetCurrentClientOrdersAsync();
             return Ok(response);
        }
        catch (OrderAccessDeniedException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, exception.Message);
        }
    }
    
    [HttpGet("/api/masters/me/orders")]
    public async Task<ActionResult<IReadOnlyCollection<MasterOrderListItemResponse>>> GetCurrentMasterOrders()
    {
        try
        {
            IReadOnlyCollection<MasterOrderListItemResponse> response =
                await _orderService.GetCurrentMasterOrdersAsync();
            return Ok(response);
        }
        catch (OrderAccessDeniedException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, exception.Message);
        }
    }

    [HttpPatch("{orderId:int}/complete")]
    public async Task<ActionResult> CompleteOrder(int orderId)
    {
        try
        {
            await _orderService.CompleteAsync(orderId);
            return NoContent();
        }
        catch (OrderNotFoundException exception)
        {
            return StatusCode(StatusCodes.Status404NotFound, exception.Message);
        }
        catch (OrderAccessDeniedException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, exception.Message);
        }
        catch (OrderCannotBeCompletedException exception)
        {
            return Conflict(exception.Message);
        }
    }
    [HttpPatch("{orderId:int}/cancel")]
    public async Task<ActionResult> CancelOrder(int orderId)
    {
        try
        {
            await _orderService.CancelAsync(orderId);
            return NoContent();
        }
        catch (OrderNotFoundException exception)
        {
            return StatusCode(StatusCodes.Status404NotFound, exception.Message);
        }
        catch (OrderAccessDeniedException exception)
        {
            return StatusCode(StatusCodes.Status403Forbidden, exception.Message);
        }
        catch (OrderCannotBeCancelledException exception)
        {
            return Conflict(exception.Message);
        }
    }
    
}