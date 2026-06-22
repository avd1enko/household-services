namespace PaymentService.Application.DTOs;

public class CreatePaymentRequest
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string CardNumber { get; set; } = null!;
}
