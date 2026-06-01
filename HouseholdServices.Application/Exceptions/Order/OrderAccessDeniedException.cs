namespace HouseholdServices.Application.Exceptions.Order;

public class OrderAccessDeniedException : Exception
{
    public OrderAccessDeniedException()
        : base("Нет доступа к этому заказу.")
    {
    }
}