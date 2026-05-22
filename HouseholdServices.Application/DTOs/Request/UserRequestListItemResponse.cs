namespace HouseholdServices.Application.DTOs.Request;

public class UserRequestListItemResponse
{
    public int RequestId { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public DateTime DesiredDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
