using HouseholdServices.Application.DTOs.ServiceCategories;

namespace HouseholdServices.Application.DTOs.MasterProfile;

public class MasterProfileResponse
{
    public int UserId { get; set; }
    public string Login { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string? Description { get; set; }
    public int ExperienceYears { get; set; }
    public List<ServiceCategoryResponse> Categories { get; set; } = [];
}