namespace HouseholdServices.Domain.Entities;

public class ServiceCategory
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}