using PaymentService.Application.DTOs;
using PaymentService.Application.Services;

namespace PaymentService.Infrastructure.Services;

public class MockPaymentService : IPaymentService
{
    public Task<PaymentStatusResponse> PayAsync(CreatePaymentRequest request)
    {
        if (request.OrderId <= 0)
        {
            return Task.FromResult(new PaymentStatusResponse
            {
                IsPaid = false,
                Status = "InvalidOrder",
                Message = "Order id must be greater than zero"
            });
        }

        if (request.Amount <= 0)
        {
            return Task.FromResult(new PaymentStatusResponse
            {
                IsPaid = false,
                Status = "InvalidAmount",
                Message = "Amount must be greater than zero"
            });
        }

        if (string.IsNullOrWhiteSpace(request.CardNumber))
        {
            return Task.FromResult(new PaymentStatusResponse
            {
                IsPaid = false,
                Status = "InvalidCard",
                Message = "Card number is required"
            });
        }

        string normalizedCard = request.CardNumber.Replace(" ", "");

        if (normalizedCard.EndsWith("0000"))
        {
            return Task.FromResult(new PaymentStatusResponse
            {
                IsPaid = false,
                Status = "Declined",
                Message = "Payment was declined by mock bank"
            });
        }

        return Task.FromResult(new PaymentStatusResponse
        {
            IsPaid = true,
            Status = "Paid",
            PaymentId = $"pay_{request.OrderId}_{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            Message = $"Payment for order {request.OrderId} was completed"
        });
    }
}
