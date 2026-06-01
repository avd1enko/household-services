namespace HouseholdServices.Application.DTOs.Order;

public class OrderResponse
{
    public int OrderId { get; set; }
    public string Status { get; set; } = null!;
    public decimal Price { get; set; }
    public DateTime? InitialMeetingAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    public int RequestId { get; set; }
    public string RequestTitle { get; set; } = null!;
    public string RequestDescription { get; set; } = null!;
    public string RequestAddress { get; set; } = null!;
    public DateTime DesiredDate { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    
    public int ClientId { get; set; }
    public string ClientFirstName { get; set; } = null!;
    public string ClientLastName { get; set; } = null!;
    public string ClientPhone { get; set; } = null!;
    
    public int MasterId { get; set; }
    public string MasterFirstName { get; set; } = null!;
    public string MasterLastName { get; set; } = null!;
    public string MasterPhone { get; set; } = null!;



}