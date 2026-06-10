namespace HouseholdServices.Application.Exceptions.Review;

public class ReviewOrderNotCompletedException : Exception
{
    public ReviewOrderNotCompletedException()
        : base("Review can be created only for a completed order")
    {
    }
}
