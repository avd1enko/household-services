using HouseholdServices.Application.DTOs.Response;
using HouseholdServices.Application.Exceptions.Response;
using HouseholdServices.Application.Services.Responses;
using HouseholdServices.Application.Services.Users;
using HouseholdServices.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HouseholdServices.Infrastructure.Services.Responses;

public class ResponseService : IResponseService
{
    private const string PendingStatusName = "pending";
    private const string OpenRequestStatusName = "open";

    private readonly HouseholdServicesDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public ResponseService(HouseholdServicesDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<ResponseForRequestListItemResponse> CreateAsync(int requestId, CreateResponseRequest request)
    {
        if (request.ProposedPrice <= 0)
            throw new InvalidResponsePriceException();

        int masterId = _currentUserService.GetUserId();

        Domain.Entities.Request? serviceRequest = await _dbContext.Requests
            .FirstOrDefaultAsync(serviceRequest => serviceRequest.RequestId == requestId);

        if (serviceRequest is null)
            throw new RequestNotFoundException();

        string? requestStatus = await _dbContext.RequestStatuses
            .Where(status => status.RequestStatusId == serviceRequest.RequestStatusId)
            .Select(status => status.Name)
            .FirstOrDefaultAsync();

        if (requestStatus != OpenRequestStatusName)
            throw new RequestNotActiveException();

        bool responseExists = await _dbContext.Responses
            .AnyAsync(response => response.RequestId == requestId && response.MasterId == masterId);

        if (responseExists)
            throw new ResponseAlreadyExistsException();

        int pendingStatusId = await _dbContext.ResponseStatuses
            .Where(status => status.Name == PendingStatusName)
            .Select(status => status.ResponseStatusId)
            .FirstOrDefaultAsync();

        if (pendingStatusId == 0)
            throw new ResponseStatusNotFoundException();

        Domain.Entities.Response response = new Domain.Entities.Response
        {
            RequestId = requestId,
            MasterId = masterId,
            ResponseStatusId = pendingStatusId,
            ProposedPrice = request.ProposedPrice,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Responses.Add(response);
        await _dbContext.SaveChangesAsync();

        return await GetResponseForRequestListItemAsync(response.ResponseId);
    }

    public async Task<IReadOnlyCollection<ResponseForRequestListItemResponse>> GetByRequestIdAsync(int requestId)
    {
        bool requestExists = await _dbContext.Requests
            .AnyAsync(serviceRequest => serviceRequest.RequestId == requestId);

        if (!requestExists)
            throw new RequestNotFoundException();

        return await _dbContext.Responses
            .Where(response => response.RequestId == requestId)
            .OrderByDescending(response => response.CreatedAt)
            .Select(response => new ResponseForRequestListItemResponse
            {
                ResponseId = response.ResponseId,
                RequestId = response.RequestId,
                Status = _dbContext.ResponseStatuses
                    .Where(status => status.ResponseStatusId == response.ResponseStatusId)
                    .Select(status => status.Name)
                    .FirstOrDefault()!,
                ProposedPrice = response.ProposedPrice,
                Comment = response.Comment,
                CreatedAt = response.CreatedAt,
                MasterId = response.MasterId,
                MasterFirstName = response.Master.FirstName,
                MasterLastName = response.Master.LastName,
                MasterPhone = response.Master.Phone,
                MasterDescription = response.Master.MasterProfile == null ? null : response.Master.MasterProfile.Description,
                MasterExperienceYears = response.Master.MasterProfile == null ? null : response.Master.MasterProfile.ExperienceYears
            })
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<MasterResponseListItemResponse>> GetCurrentMasterResponsesAsync()
    {
        int masterId = _currentUserService.GetUserId();

        return await _dbContext.Responses
            .Where(response => response.MasterId == masterId)
            .OrderByDescending(response => response.CreatedAt)
            .Select(response => new MasterResponseListItemResponse
            {
                ResponseId = response.ResponseId,
                RequestId = response.RequestId,
                RequestTitle = response.Request.Title,
                CategoryName = _dbContext.ServiceCategories
                    .Where(category => category.CategoryId == response.Request.CategoryId)
                    .Select(category => category.Name)
                    .FirstOrDefault()!,
                Status = _dbContext.ResponseStatuses
                    .Where(status => status.ResponseStatusId == response.ResponseStatusId)
                    .Select(status => status.Name)
                    .FirstOrDefault()!,
                ProposedPrice = response.ProposedPrice,
                Comment = response.Comment,
                CreatedAt = response.CreatedAt
            })
            .ToListAsync();
    }

    private async Task<ResponseForRequestListItemResponse> GetResponseForRequestListItemAsync(int responseId)
    {
        return await _dbContext.Responses
            .Where(response => response.ResponseId == responseId)
            .Select(response => new ResponseForRequestListItemResponse
            {
                ResponseId = response.ResponseId,
                RequestId = response.RequestId,
                Status = _dbContext.ResponseStatuses
                    .Where(status => status.ResponseStatusId == response.ResponseStatusId)
                    .Select(status => status.Name)
                    .FirstOrDefault()!,
                ProposedPrice = response.ProposedPrice,
                Comment = response.Comment,
                CreatedAt = response.CreatedAt,
                MasterId = response.MasterId,
                MasterFirstName = response.Master.FirstName,
                MasterLastName = response.Master.LastName,
                MasterPhone = response.Master.Phone,
                MasterDescription = response.Master.MasterProfile == null ? null : response.Master.MasterProfile.Description,
                MasterExperienceYears = response.Master.MasterProfile == null ? null : response.Master.MasterProfile.ExperienceYears
            })
            .FirstAsync();
    }
}
