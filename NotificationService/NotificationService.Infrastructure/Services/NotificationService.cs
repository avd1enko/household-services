using NotificationService.Application.Services;
using NotificationService.Application.DTOs;
namespace NotificationService.Infrastructure.Services;

public class SmsNotificationService : ISmsNotificationService
{
    public Task<NotificationStatusResponse> SendAsyncSms(SendNotificationRequest request)
    {
//создает уже завершенный таск с готовым результатом
//(скорее для шаблона/тестирования/асинк формата без реального асинк кода)

        string phone = request.PhoneNumber;
        string message = request.Message;

        if (string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(message))
            return Task.FromResult(new NotificationStatusResponse
            {
                IsSent = false,
                Message = "Incorrect data"
            });
        
        return Task.FromResult(
            new NotificationStatusResponse {
            IsSent = true,
            Message = $"Message is sent to {phone}"
        });
    }
    
}