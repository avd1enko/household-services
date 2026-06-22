namespace HouseholdServices.Application.Exceptions.Order;

public class OrderCannotBePaidException : Exception
{
    public OrderCannotBePaidException()
        : base("This order cannot be paid.")
    {
    }
}
