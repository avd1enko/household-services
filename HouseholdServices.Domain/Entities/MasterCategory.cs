namespace HouseholdServices.Domain.Entities;

public class MasterCategory
{
    public int UserId { get; set; }
    public int CategoryId { get; set; }

    public User User { get; set; } = null!;
    public ServiceCategory Category { get; set; } = null!;
}
