using HouseholdServices.Application.DTOs.Response;

namespace HouseholdServices.Application.Services.Responses;

public interface IResponseService
{
    Task<ResponseForRequestListItemResponse> CreateAsync(int requestId, CreateResponseRequest request);
    Task<IReadOnlyCollection<ResponseForRequestListItemResponse>> GetByRequestIdAsync(int requestId);
    Task<IReadOnlyCollection<MasterResponseListItemResponse>> GetCurrentMasterResponsesAsync();
}
