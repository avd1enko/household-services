namespace HouseholdServices.Application.Exceptions.Request;

public class ClientRoleRequiredException : Exception
{
    public ClientRoleRequiredException()
        : base("Only clients can create requests")
    {
    }
}
