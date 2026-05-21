using System.Threading.Tasks;
using HouseholdServices.Application.DTOs.Notification;
using HouseholdServices.Application.Services.Notification;

namespace HouseholdServices.Infrastructure.Services.Notification;

public class NotificationTestService : INotificationTestService
{
    private readonly INotificationClient _notificationClient;

    public NotificationTestService(INotificationClient notificationClient)
    {
        _notificationClient = notificationClient;
    }

    public async Task SendTestNotificationAsync()
    {
        NotificationInfoRequest request = new NotificationInfoRequest
        {
            PhoneNumber = "+1234567890",
            Message = "Test Notification"
        };

        await _notificationClient.NotifyUserAsync(request);
    }
}