using HouseholdServices.Application.Services.Notification;
using HouseholdServices.Application.DTOs.Notification;
using System.Net.Http.Json;

namespace HouseholdServices.Infrastructure.Services.Notification;

public class NotificationClient : INotificationClient
{
    private readonly HttpClient _httpClient;

    public NotificationClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<NotificationStatusResponse> NotifyUserAsync(NotificationInfoRequest request)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            "api/notification/send",
            request); // указываем куда и что отправляем
        response.EnsureSuccessStatusCode(); // проверка кода ответа, если не норм, то выбрасываем исключение (не зря поменяли коды в нотификейшн бекенде!)
        NotificationStatusResponse? result =
            await response.Content.ReadFromJsonAsync<NotificationStatusResponse>();
        if (result is null)
            throw new InvalidOperationException("Notification service returned empty response");
        return result;
    }
}