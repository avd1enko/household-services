namespace HouseholdServices.Application.DTOs.UserProfile;

public class UserProfileResponse
{
    public int UserId { get; set; }
    public string Login { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Role { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public int DaysSinceRegistration { get; set; }
    public int CompletedOrdersCount { get; set; }
}