using System.Threading.Tasks;

namespace HouseholdServices.Application.Services.Notification;

public interface INotificationTestService
{
    Task SendTestNotificationAsync();
}