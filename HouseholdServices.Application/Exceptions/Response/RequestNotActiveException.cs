namespace HouseholdServices.Application.Exceptions.Response;

public class RequestNotActiveException : Exception
{
    public RequestNotActiveException() : base("Request is not active")
    {
    }
}
