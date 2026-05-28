namespace HouseholdServices.Application.DTOs.Request;

public class RequestFilterRequest
{
    public int? CategoryId { get; set; }
    public DateTime? DesiredDateFrom { get; set; }
    public DateTime? DesiredDateTo { get; set; }
    public DateTime? CreatedAtFrom { get; set; }
    public DateTime? CreatedAtTo { get; set; }
    public string? Title { get; set; }
}
