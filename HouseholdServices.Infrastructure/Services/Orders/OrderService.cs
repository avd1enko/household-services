using HouseholdServices.Application.Services.Order;
using HouseholdServices.Application.DTOs.Order;
using HouseholdServices.Application.Services.Users;
using HouseholdServices.Domain.Entities;
using HouseholdServices.Infrastructure.Data;
using HouseholdServices.Application.Exceptions.Order;
using Microsoft.EntityFrameworkCore;

namespace HouseholdServices.Infrastructure.Services.Orders;

public class OrderService : IOrderService
{
    private const string ClientRoleName = "client";
    private const string MasterRoleName = "master";
    private const string InProgressStatusName = "in_progress";
    private const string CompletedStatusName = "completed";
    private const string CancelledStatusName = "cancelled";

    private readonly HouseholdServicesDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public OrderService(HouseholdServicesDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<OrderResponse> GetByIdAsync(int orderId)
    {
        OrderView? orderView = await _dbContext.OrderViews
            .AsNoTracking()
            .FirstOrDefaultAsync(order => order.OrderId == orderId);

        if (orderView == null)
            throw new OrderNotFoundException();

        int currentUserId = _currentUserService.GetUserId();
        string currentUserRole = _currentUserService.GetRole();

        bool clientHasAccess = currentUserId == orderView.ClientId && currentUserRole == ClientRoleName;
        bool masterHasAccess = currentUserId == orderView.MasterId && currentUserRole == MasterRoleName;

        if (!clientHasAccess && !masterHasAccess)
            throw new OrderAccessDeniedException();

        return new OrderResponse
        {
            OrderId = orderView.OrderId,
            Status = orderView.Status,
            Price = orderView.Price,
            InitialMeetingAt = orderView.InitialMeetingAt,
            CreatedAt = orderView.CreatedAt,
            CompletedAt = orderView.CompletedAt,

            RequestId = orderView.RequestId,
            RequestTitle = orderView.RequestTitle,
            RequestDescription = orderView.RequestDescription,
            RequestAddress = orderView.RequestAddress,
            DesiredDate = orderView.DesiredDate,
            CategoryId = orderView.CategoryId,
            CategoryName = orderView.CategoryName,

            ClientId = orderView.ClientId,
            ClientFirstName = orderView.ClientFirstName,
            ClientLastName = orderView.ClientLastName,
            ClientPhone = orderView.ClientPhone,

            MasterId = orderView.MasterId,
            MasterFirstName = orderView.MasterFirstName,
            MasterLastName = orderView.MasterLastName,
            MasterPhone = orderView.MasterPhone
        };
    }

    public async Task<IReadOnlyCollection<UserOrderListItemResponse>> GetCurrentClientOrdersAsync()
    {
        int currentUserId = _currentUserService.GetUserId();
        string currentUserRole = _currentUserService.GetRole();

        if (currentUserRole != ClientRoleName)
            throw new OrderAccessDeniedException();

        List<UserOrderListItemResponse> orders = await _dbContext.OrderViews
            .AsNoTracking()
            .Where(order => order.ClientId == currentUserId)
            .Select(order => new UserOrderListItemResponse
            {
                OrderId = order.OrderId,
                Status = order.Status,
                Price = order.Price,
                InitialMeetingAt = order.InitialMeetingAt,
                CreatedAt = order.CreatedAt,
                CompletedAt = order.CompletedAt,
                RequestId = order.RequestId,
                RequestTitle = order.RequestTitle,
                CategoryName = order.CategoryName,
                MasterId = order.MasterId,
                MasterFirstName = order.MasterFirstName,
                MasterLastName = order.MasterLastName
            })
            .ToListAsync();

        return orders;
    }

    public async Task<IReadOnlyCollection<MasterOrderListItemResponse>> GetCurrentMasterOrdersAsync()
    {
        int currentUserId = _currentUserService.GetUserId();
        string currentUserRole = _currentUserService.GetRole();

        if (currentUserRole != MasterRoleName)
            throw new OrderAccessDeniedException();

        List<MasterOrderListItemResponse> orders = await _dbContext.OrderViews
            .AsNoTracking()
            .Where(order => order.MasterId == currentUserId)
            .Select(order => new MasterOrderListItemResponse
            {
                OrderId = order.OrderId,
                Status = order.Status,
                Price = order.Price,
                InitialMeetingAt = order.InitialMeetingAt,
                CreatedAt = order.CreatedAt,
                CompletedAt = order.CompletedAt,
                RequestId = order.RequestId,
                RequestTitle = order.RequestTitle,
                CategoryName = order.CategoryName,
                ClientId = order.ClientId,
                ClientFirstName = order.ClientFirstName,
                ClientLastName = order.ClientLastName,
                ClientPhone = order.ClientPhone
            })
            .ToListAsync();

        return orders;
    }

    public async Task CompleteAsync(int orderId)
    {
        Order? order = await _dbContext.Orders
            .Include(order => order.Response)
            .ThenInclude(response => response.Request)
            .FirstOrDefaultAsync(order => order.OrderId == orderId);

        if (order == null)
            throw new OrderNotFoundException();

        int currentUserId = _currentUserService.GetUserId();
        string currentUserRole = _currentUserService.GetRole();

        if (currentUserRole != ClientRoleName || order.Response.Request.ClientId != currentUserId)
            throw new OrderAccessDeniedException();

        int inProgressStatusId = await GetOrderStatusIdAsync(InProgressStatusName);
        int completedStatusId = await GetOrderStatusIdAsync(CompletedStatusName);

        if (order.OrderStatusId != inProgressStatusId)
            throw new OrderCannotBeCompletedException();

        order.OrderStatusId = completedStatusId;
        order.CompletedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task CancelAsync(int orderId)
    {
        Order? order = await _dbContext.Orders
            .Include(order => order.Response)
            .ThenInclude(response => response.Request)
            .FirstOrDefaultAsync(order => order.OrderId == orderId);

        if (order == null)
            throw new OrderNotFoundException();

        int currentUserId = _currentUserService.GetUserId();
        string currentUserRole = _currentUserService.GetRole();

        bool clientHasAccess = currentUserRole == ClientRoleName && order.Response.Request.ClientId == currentUserId;
        bool masterHasAccess = currentUserRole == MasterRoleName && order.Response.MasterId == currentUserId;

        if (!clientHasAccess && !masterHasAccess)
            throw new OrderAccessDeniedException();

        int inProgressStatusId = await GetOrderStatusIdAsync(InProgressStatusName);
        int cancelledStatusId = await GetOrderStatusIdAsync(CancelledStatusName);

        if (order.OrderStatusId != inProgressStatusId)
            throw new OrderCannotBeCancelledException();

        order.OrderStatusId = cancelledStatusId;
        await _dbContext.SaveChangesAsync();
    }

    private async Task<int> GetOrderStatusIdAsync(string statusName)
    {
        int statusId = await _dbContext.OrderStatuses
            .Where(status => status.Name == statusName)
            .Select(status => status.OrderStatusId)
            .FirstOrDefaultAsync();

        if (statusId == 0)
            throw new OrderStatusNotFoundException();

        return statusId;
    }
}
