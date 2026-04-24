namespace HouseholdServices.Domain.Entities;

public class ServiceCategory
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public ICollection<MasterCategory> MasterCategories { get; set; } = new List<MasterCategory>();
}
