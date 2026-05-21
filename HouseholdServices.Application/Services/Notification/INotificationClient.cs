using HouseholdServices.Application.DTOs.Notification;

namespace HouseholdServices.Application.Services.Notification;

public interface INotificationClient
{
    Task<NotificationStatusResponse> NotifyUserAsync(NotificationInfoRequest request);
}