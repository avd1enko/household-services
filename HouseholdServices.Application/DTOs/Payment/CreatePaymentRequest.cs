namespace HouseholdServices.Application.DTOs.Payment;

public class CreatePaymentRequest
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string CardNumber { get; set; } = null!;
}
