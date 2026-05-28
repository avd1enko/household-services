namespace HouseholdServices.Application.Exceptions.Response;

public class RequestNotFoundException : Exception
{
    public RequestNotFoundException() : base("Request not found")
    {
    }
}
