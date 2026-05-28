namespace HouseholdServices.Application.Exceptions.Response;

public class ResponseNotFoundException : Exception
{
    public ResponseNotFoundException() : base("Response not found")
    {
    }
}
