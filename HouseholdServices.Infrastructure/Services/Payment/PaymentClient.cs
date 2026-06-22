using System.Net.Http.Json;
using HouseholdServices.Application.DTOs.Payment;
using HouseholdServices.Application.Services.Payment;

namespace HouseholdServices.Infrastructure.Services.Payment;

public class PaymentClient : IPaymentClient
{
    private readonly HttpClient _httpClient;

    public PaymentClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PaymentStatusResponse> PayAsync(CreatePaymentRequest request)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/payment/pay", request);

        PaymentStatusResponse? result = await response.Content.ReadFromJsonAsync<PaymentStatusResponse>();
        if (result is null)
        {
            response.EnsureSuccessStatusCode();
            throw new InvalidOperationException("Payment service returned empty response");
        }

        return result;
    }
}
