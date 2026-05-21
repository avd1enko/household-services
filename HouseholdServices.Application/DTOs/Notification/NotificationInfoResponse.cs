namespace HouseholdServices.Application.DTOs.Notification;

public class NotificationInfoRequest
{
    public string PhoneNumber { get; set; } = null!;
    public string Message { get; set; } = null!;
}