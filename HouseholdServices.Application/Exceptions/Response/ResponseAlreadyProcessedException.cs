namespace HouseholdServices.Application.Exceptions.Response;

public class ResponseAlreadyProcessedException : Exception
{
    public ResponseAlreadyProcessedException() : base("Response already accepted or rejected")
    {
    }
}
