namespace HouseholdServices.Domain.Entities;

public class Response
{
    public int ResponseId { get; set; }
    public int RequestId { get; set; }
    public int MasterId { get; set; }
    public int ResponseStatusId { get; set; }
    public decimal ProposedPrice { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public Request Request { get; set; } = null!;
    public User Master { get; set; } = null!;
    public Order? Order { get; set; }
}
