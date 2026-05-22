namespace HouseholdServices.Application.Exceptions.Request;

public class RequestAccessDeniedException : Exception
{
    public RequestAccessDeniedException()
        : base("You do not have access to this request")
    {
    }
}
