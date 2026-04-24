namespace HouseholdServices.Domain.Entities;

public class Request
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
    public User Client { get; set; } = null!;
    public ICollection<Response> Responses { get; set; } = new List<Response>();
}
