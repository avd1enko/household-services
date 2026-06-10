namespace HouseholdServices.Application.DTOs.Review;

public class ReviewResponse
{
    public int ReviewId { get; set; }
    public int OrderId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }

    public int RequestId { get; set; }
    public string RequestTitle { get; set; } = null!;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;

    public int ClientId { get; set; }
    public string ClientFirstName { get; set; } = null!;
    public string ClientLastName { get; set; } = null!;

    public int MasterId { get; set; }
    public string MasterFirstName { get; set; } = null!;
    public string MasterLastName { get; set; } = null!;
}
