using HouseholdServices.Application.DTOs.Order;
using HouseholdServices.Application.DTOs.Payment;

namespace HouseholdServices.Application.Services.Order;

public interface IOrderService
{
    Task<OrderResponse> GetByIdAsync(int orderId);
    Task<IReadOnlyCollection<UserOrderListItemResponse>> GetCurrentClientOrdersAsync();
    Task<IReadOnlyCollection<MasterOrderListItemResponse>> GetCurrentMasterOrdersAsync();
    Task UpdateInitialMeetingAsync(int orderId, UpdateOrderInitialMeetingRequest request);
    Task<PaymentStatusResponse> PayAsync(int orderId, PayOrderRequest request);
    Task CompleteAsync(int orderId);
    Task CancelAsync(int orderId);
}
