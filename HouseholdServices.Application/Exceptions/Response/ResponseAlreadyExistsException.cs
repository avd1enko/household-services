namespace HouseholdServices.Application.Exceptions.Response;

public class ResponseAlreadyExistsException : Exception
{
    public ResponseAlreadyExistsException() : base("Response already exists")
    {
    }
}
