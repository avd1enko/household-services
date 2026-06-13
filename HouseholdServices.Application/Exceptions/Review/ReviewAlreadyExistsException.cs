namespace HouseholdServices.Application.Exceptions.Review;

public class ReviewAlreadyExistsException : Exception
{
    public ReviewAlreadyExistsException()
        : base("Review for this order already exists")
    {
    }
}
