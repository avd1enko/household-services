namespace HouseholdServices.Application.Exceptions.Request;

public class RequestCannotBeCancelledException : Exception
{
    public RequestCannotBeCancelledException()
        : base("Request cannot be cancelled")
    {
    }
}
