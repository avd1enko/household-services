namespace HouseholdServices.Domain.Entities;

public class MasterProfile
{
    public int UserId { get; set; }
    public string Description { get; set; } = null!;
    public int ExperienceYears { get; set; }

    public User User { get; set; } = null!;
}
