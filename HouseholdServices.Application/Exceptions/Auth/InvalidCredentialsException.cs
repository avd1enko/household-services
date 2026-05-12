namespace HouseholdServices.Application.Exceptions.Auth;

public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException()
        : base("Incorrect login or password")
    {
    }
}