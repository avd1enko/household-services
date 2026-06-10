namespace HouseholdServices.Application.Exceptions.Review;

public class InvalidReviewRatingException : Exception
{
    public InvalidReviewRatingException()
        : base("Review rating must be between 1 and 5")
    {
    }
}
