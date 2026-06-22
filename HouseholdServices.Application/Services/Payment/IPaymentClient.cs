using HouseholdServices.Application.DTOs.Payment;

namespace HouseholdServices.Application.Services.Payment;

public interface IPaymentClient
{
    Task<PaymentStatusResponse> PayAsync(CreatePaymentRequest request);
}
