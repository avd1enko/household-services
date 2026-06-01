namespace HouseholdServices.Application.Exceptions.Order;

public class OrderNotFoundException : Exception
{
    public OrderNotFoundException()
        : base("Заказ не найден.")
    {
    }
}