namespace HouseholdServices.Application.DTOs.Response;

public class ResponseForRequestListItemResponse
{
    public int ResponseId { get; set; }
    public int RequestId { get; set; }
    public string Status { get; set; } = null!;
    public decimal ProposedPrice { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MasterId { get; set; }
    public string MasterFirstName { get; set; } = null!;
    public string MasterLastName { get; set; } = null!;
    public string MasterPhone { get; set; } = null!;
    public string? MasterDescription { get; set; }
    public int? MasterExperienceYears { get; set; }
}
