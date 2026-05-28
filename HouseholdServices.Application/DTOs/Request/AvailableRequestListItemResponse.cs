namespace HouseholdServices.Application.DTOs.Request;

public class AvailableRequestListItemResponse
{
    public int RequestId { get; set; }
    public int ClientId { get; set; }
    public string ClientFirstName { get; set; } = null!;
    public string ClientLastName { get; set; } = null!;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Address { get; set; } = null!;
    public DateTime DesiredDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = null!;
}
