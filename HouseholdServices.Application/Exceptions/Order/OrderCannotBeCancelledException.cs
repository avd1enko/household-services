namespace HouseholdServices.Application.Exceptions.Order;

public class OrderCannotBeCancelledException : Exception
{
    public OrderCannotBeCancelledException()
        : base("Этот заказ нельзя отменить.")
    {
    }
}