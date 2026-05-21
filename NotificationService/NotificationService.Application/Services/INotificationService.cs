using NotificationService.Application.DTOs;
namespace NotificationService.Application.Services;

public interface ISmsNotificationService
{
    Task<NotificationStatusResponse> SendAsyncSms(SendNotificationRequest request);
}