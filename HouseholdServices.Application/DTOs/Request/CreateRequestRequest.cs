namespace HouseholdServices.Application.DTOs.Request;

public class CreateRequestRequest
{
    public int CategoryId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Address { get; set; } = null!;
    public DateTime DesiredDate { get; set; }
}
