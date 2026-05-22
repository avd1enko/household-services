namespace HouseholdServices.Application.Exceptions.Response;

public class RequestNotAvailableForMasterException : Exception
{
    public RequestNotAvailableForMasterException() : base("Request is not available for this master")
    {
    }
}
