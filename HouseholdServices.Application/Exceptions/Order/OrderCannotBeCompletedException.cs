namespace HouseholdServices.Application.Exceptions.Order;

public class OrderCannotBeCompletedException : Exception
{
    public OrderCannotBeCompletedException()
        : base("Этот заказ нельзя завершить.")
    {
    }
}