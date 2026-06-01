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

        bool clientHasAccess = (currentUserId == orderView.ClientId && currentUserRole == "client");
        bool masterHasAccess = (currentUserId == orderView.MasterId && currentUserRole == "master");

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

        if (currentUserRole != "client")
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

        if (currentUserRole != "master")
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

        if (currentUserRole != "client" || order.Response.Request.ClientId != currentUserId)
        {
            throw new OrderAccessDeniedException();
        }

        if (order.OrderStatusId != 1) // not in_progress
            throw new OrderCannotBeCompletedException();

        order.OrderStatusId = 3;

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

        bool clientHasAccess = (currentUserRole == "client" && order.Response.Request.ClientId == currentUserId);
        bool masterHasAccess = (currentUserRole == "master" && order.Response.MasterId == currentUserId);

        if (!clientHasAccess && !masterHasAccess)
        {
            throw new OrderAccessDeniedException();
        }

        if (order.OrderStatusId != 1) // not in_progress
        {
            throw new OrderCannotBeCancelledException();
        }

        order.OrderStatusId = 3; // cancelled
        await _dbContext.SaveChangesAsync();
    }
}