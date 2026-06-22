namespace PaymentService.Application.DTOs;

public class PaymentStatusResponse
{
    public bool IsPaid { get; set; }
    public string Status { get; set; } = null!;
    public string? PaymentId { get; set; }
    public string? Message { get; set; }
}
