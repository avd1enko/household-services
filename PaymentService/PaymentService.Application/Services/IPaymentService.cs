using PaymentService.Application.DTOs;

namespace PaymentService.Application.Services;

public interface IPaymentService
{
    Task<PaymentStatusResponse> PayAsync(CreatePaymentRequest request);
}
