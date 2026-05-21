using System.Threading.Tasks;
using HouseholdServices.Application.Services.Notification;
using Microsoft.AspNetCore.Mvc;

namespace HouseholdServices.API.Controllers;

[ApiController]
[Route("api/notification-test")]
public class NotificationTestController : ControllerBase
{
    private readonly INotificationTestService _notificationTestService;

    public NotificationTestController(INotificationTestService notificationTestService)
    {
        _notificationTestService = notificationTestService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendAsync()
    {
        await _notificationTestService.SendTestNotificationAsync();
        return Ok("Test notification sent");
    }
}