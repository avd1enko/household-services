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
    public MasterProfile? MasterProfile { get; set; }
    // navigation property на множесто сущнстей (один ко многим)
    public ICollection<Request> ClientRequests { get; set; } = new List<Request>();
    public ICollection<Response> MasterResponses { get; set; } = new List<Response>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<MasterCategory> MasterCategories { get; set; } = new List<MasterCategory>();
}
