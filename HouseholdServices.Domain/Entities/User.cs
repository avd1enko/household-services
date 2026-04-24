namespace HouseholdServices.Domain.Entities;

public class User
{
    public int UserId { get; set; }
    public string Login { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    
    public ICollection<Request> ClientRequests { get; set; } = new List<Request>();
    public ICollection<Response> MasterResponses { get; set; } = new List<Response>();
}
