namespace HouseholdServices.Application.Exceptions.Response;

public class ResponseStatusNotFoundException : Exception
{
    public ResponseStatusNotFoundException() : base("Response status not found")
    {
    }
}
