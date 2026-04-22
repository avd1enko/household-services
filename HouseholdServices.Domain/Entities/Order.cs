namespace HouseholdServices.Domain.Entities;

public class Order
{
    public int OrderId { get; set; }
    public int ResponseId { get; set; }
    public int OrderStatusId { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Response Response { get; set; } = null!;
    public Review? Review { get; set; }
}
