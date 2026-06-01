namespace HouseholdServices.Application.Exceptions.Order;

public class OrderStatusNotFoundException : Exception
{
    public OrderStatusNotFoundException()
        : base("Статус заказа не найден.")
    {
    }
}