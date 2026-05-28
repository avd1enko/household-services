using HouseholdServices.Application.DTOs.Request;

namespace HouseholdServices.Application.Services.Request;

public interface IRequestService
{
    Task<RequestResponse> CreateAsync(CreateRequestRequest request);
    Task<RequestResponse> GetByIdAsync(int requestId);
    Task<List<AvailableRequestListItemResponse>> GetAvailableForCurrentMasterAsync(RequestFilterRequest filter);
    Task<List<UserRequestListItemResponse>> GetCurrentUserRequestsAsync(RequestFilterRequest filter);
    Task CancelAsync(int requestId);
}
