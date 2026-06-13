namespace HouseholdServices.Application.Exceptions.Order;

public class OrderCannotBeUpdatedException : Exception
{
    public OrderCannotBeUpdatedException()
        : base("Order cannot be updated in its current status.")
    {
    }
}