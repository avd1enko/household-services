using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.DTOs;
using NotificationService.Application.Services;




namespace NotificationService.API.Controllers;

[ApiController]
[Route("api/notification")]
public class NotificationController : ControllerBase
{
    private readonly ISmsNotificationService _notificationService;

    public NotificationController(ISmsNotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost("send")]
    public async Task<ActionResult<NotificationStatusResponse>> SendAsync(SendNotificationRequest request)
    {
        NotificationStatusResponse response = await _notificationService.SendAsyncSms(request);

        if (response.IsSent)
            return Ok(response);
        return BadRequest(response); // чтоб код 400 был, а не 200 на ошибке

    }
}