using HouseholdServices.Application.DTOs.Review;
using HouseholdServices.Application.Exceptions.Review;
using HouseholdServices.Application.Services.Reviews;
using HouseholdServices.Application.Services.Users;
using HouseholdServices.Domain.Entities;
using HouseholdServices.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HouseholdServices.Infrastructure.Services.Reviews;

public class ReviewService : IReviewService
{
    private const string ClientRoleName = "client";
    private const string CompletedOrderStatusName = "completed";

    private readonly HouseholdServicesDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public ReviewService(HouseholdServicesDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<ReviewResponse> CreateAsync(int orderId, CreateReviewRequest request)
    {
        if (_currentUserService.GetRole() != ClientRoleName)
            throw new ReviewAccessDeniedException();

        if (request.Rating < 1 || request.Rating > 5)
            throw new InvalidReviewRatingException();

        int clientId = _currentUserService.GetUserId();

        Order? order = await _dbContext.Orders
            .Include(order => order.Response)
            .ThenInclude(response => response.Request)
            .FirstOrDefaultAsync(order => order.OrderId == orderId);

        if (order is null)
            throw new ReviewOrderNotFoundException();

        if (order.Response.Request.ClientId != clientId)
            throw new ReviewAccessDeniedException();

        string? orderStatus = await _dbContext.OrderStatuses
            .Where(status => status.OrderStatusId == order.OrderStatusId)
            .Select(status => status.Name)
            .FirstOrDefaultAsync();

        if (orderStatus != CompletedOrderStatusName)
            throw new ReviewOrderNotCompletedException();

        bool reviewExists = await _dbContext.Reviews
            .AnyAsync(review => review.OrderId == orderId);

        if (reviewExists)
            throw new ReviewAlreadyExistsException();

        var review = new Review
        {
            OrderId = orderId,
            Rating = request.Rating,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Reviews.Add(review);
        await _dbContext.SaveChangesAsync();

        return await GetReviewResponseAsync(review.ReviewId);
    }

    public async Task<IReadOnlyCollection<MasterReviewListItemResponse>> GetByMasterIdAsync(int masterId)
    {
        return await _dbContext.MasterReviewViews
            .AsNoTracking()
            .Where(review => review.MasterId == masterId)
            .OrderByDescending(review => review.ReviewCreatedAt)
            .Select(review => new MasterReviewListItemResponse
            {
                ReviewId = review.ReviewId,
                OrderId = review.OrderId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.ReviewCreatedAt,
                OrderCompletedAt = review.OrderCompletedAt,
                RequestId = review.RequestId,
                RequestTitle = review.RequestTitle,
                CategoryId = review.CategoryId,
                CategoryName = review.CategoryName,
                ClientId = review.ClientId,
                ClientFirstName = review.ClientFirstName,
                ClientLastName = review.ClientLastName
            })
            .ToListAsync();
    }

    private async Task<ReviewResponse> GetReviewResponseAsync(int reviewId)
    {
        return await _dbContext.MasterReviewViews
            .AsNoTracking()
            .Where(review => review.ReviewId == reviewId)
            .Select(review => new ReviewResponse
            {
                ReviewId = review.ReviewId,
                OrderId = review.OrderId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.ReviewCreatedAt,
                RequestId = review.RequestId,
                RequestTitle = review.RequestTitle,
                CategoryId = review.CategoryId,
                CategoryName = review.CategoryName,
                ClientId = review.ClientId,
                ClientFirstName = review.ClientFirstName,
                ClientLastName = review.ClientLastName,
                MasterId = review.MasterId,
                MasterFirstName = review.MasterFirstName,
                MasterLastName = review.MasterLastName
            })
            .FirstAsync();
    }
}
