using HouseholdServices.Application.DTOs.Payment;
using HouseholdServices.Application.Services.Payment;

namespace HouseholdServices.Infrastructure.Services.Payment;

public class PaymentTestService : IPaymentTestService
{
    private readonly IPaymentClient _paymentClient;

    public PaymentTestService(IPaymentClient paymentClient)
    {
        _paymentClient = paymentClient;
    }

    public Task<PaymentStatusResponse> PayTestOrderAsync()
    {
        CreatePaymentRequest request = new CreatePaymentRequest
        {
            OrderId = 1,
            Amount = 1500,
            CardNumber = "4111 1111 1111 1111"
        };

        return _paymentClient.PayAsync(request);
    }
}
