using HouseholdServices.Application.DTOs.Review;

namespace HouseholdServices.Application.Services.Reviews;

public interface IReviewService
{
    Task<ReviewResponse> CreateAsync(int orderId, CreateReviewRequest request);
    Task<IReadOnlyCollection<MasterReviewListItemResponse>> GetByMasterIdAsync(int masterId);
}
