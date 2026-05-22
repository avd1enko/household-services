namespace HouseholdServices.Application.DTOs.Response;

public class MasterResponseListItemResponse
{
    public int ResponseId { get; set; }
    public int RequestId { get; set; }
    public string RequestTitle { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public decimal ProposedPrice { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
