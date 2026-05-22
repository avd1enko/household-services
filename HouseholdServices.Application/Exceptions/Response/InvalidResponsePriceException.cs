namespace HouseholdServices.Application.Exceptions.Response;

public class InvalidResponsePriceException : Exception
{
    public InvalidResponsePriceException() : base("Response price must be greater than zero")
    {
    }
}
