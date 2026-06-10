namespace HouseholdServices.Application.Exceptions.Review;

public class ReviewOrderNotFoundException : Exception
{
    public ReviewOrderNotFoundException()
        : base("Order not found")
    {
    }
}
