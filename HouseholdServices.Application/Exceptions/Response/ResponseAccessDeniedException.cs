namespace HouseholdServices.Application.Exceptions.Response;

public class ResponseAccessDeniedException : Exception
{
    public ResponseAccessDeniedException() : base("Access denied")
    {
    }
}
