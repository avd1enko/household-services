namespace HouseholdServices.Application.Exceptions.Auth;

public class RoleNotFoundException : Exception
{
    public RoleNotFoundException()
        : base("This role does not exist")
    {
    }
}