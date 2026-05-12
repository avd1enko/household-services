namespace HouseholdServices.Application.Exceptions.Auth;

public class LoginAlreadyTakenException : Exception
{
    public LoginAlreadyTakenException()
        : base("This login has already been taken")
    {
    }
}