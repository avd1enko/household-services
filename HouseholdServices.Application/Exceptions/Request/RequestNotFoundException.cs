namespace HouseholdServices.Application.Exceptions.Request;

public class RequestNotFoundException : Exception
{
    public RequestNotFoundException()
        : base("Request not found")
    {
    }
}
