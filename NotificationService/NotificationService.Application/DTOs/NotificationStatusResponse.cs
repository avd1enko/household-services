namespace NotificationService.Application.DTOs;

public class NotificationStatusResponse
{
    public bool IsSent { get; set; }
    public string? Message { get; set; }
}