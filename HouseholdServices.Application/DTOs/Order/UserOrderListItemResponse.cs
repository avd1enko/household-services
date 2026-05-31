namespace HouseholdServices.Application.DTOs.Order;

public class UserOrderListItemResponse
{
    public int OrderId { get; set; }
    public string Status { get; set; } = null!;
    public decimal Price { get; set; }
    public DateTime InitialMeetingAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public int RequestId { get; set; }
    public string RequestTitle { get; set; } = null!;
    public string CategoryName { get; set; } = null!;

    public int MasterId { get; set; }
    public string MasterFirstName { get; set; } = null!;
    public string MasterLastName { get; set; } = null!;
}