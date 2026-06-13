namespace HouseholdServices.Application.Exceptions.Review;

public class ReviewAccessDeniedException : Exception
{
    public ReviewAccessDeniedException()
        : base("Access denied")
    {
    }
}
