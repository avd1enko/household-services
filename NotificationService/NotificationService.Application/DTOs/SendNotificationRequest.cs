namespace NotificationService.Application.DTOs;

public class SendNotificationRequest
{
    public string PhoneNumber { get; set; } = null!;
    public string Message { get; set; } = null!;
}