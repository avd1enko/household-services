using HouseholdServices.Application.DTOs.Payment;

namespace HouseholdServices.Application.Services.Payment;

public interface IPaymentTestService
{
    Task<PaymentStatusResponse> PayTestOrderAsync();
}
