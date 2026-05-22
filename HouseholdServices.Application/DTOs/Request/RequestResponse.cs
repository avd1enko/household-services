namespace HouseholdServices.Application.DTOs.Request;

public class RequestResponse
{
    public int RequestId { get; set; }
    public int ClientId { get; set; }
    public int CategoryId { get; set; }
    public int RequestStatusId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Address { get; set; } = null!;
    public DateTime DesiredDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
