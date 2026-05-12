namespace HouseholdServices.Application.Services.Users;

public interface ICurrentUserService
{
    int GetUserId();
    string GetRole();
}