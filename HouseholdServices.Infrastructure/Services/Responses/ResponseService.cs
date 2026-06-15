using HouseholdServices.Application.DTOs.Notification;
using HouseholdServices.Application.DTOs.Response;
using HouseholdServices.Application.Exceptions.Response;
using HouseholdServices.Application.Services.Notification;
using HouseholdServices.Application.Services.Responses;
using HouseholdServices.Application.Services.Users;
using HouseholdServices.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HouseholdServices.Infrastructure.Services.Responses;

public class ResponseService : IResponseService
{
    private const string PendingStatusName = "pending";
    private const string AcceptedStatusName = "accepted";
    private const string RejectedStatusName = "rejected";
    private const string CancelledStatusName = "cancelled";
    private const string OpenRequestStatusName = "open";
    private const string ClientRoleName = "client";
    private const string MasterRoleName = "master";

    private readonly HouseholdServicesDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationClient _notificationClient;

    public ResponseService(HouseholdServicesDbContext dbContext, ICurrentUserService currentUserService, INotificationClient notificationClient)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _notificationClient = notificationClient;
    }

    public async Task<ResponseForRequestListItemResponse> CreateAsync(int requestId, CreateResponseRequest request)
    {
        if (_currentUserService.GetRole() != MasterRoleName)
            throw new ResponseAccessDeniedException();

        if (request.ProposedPrice <= 0)
            throw new InvalidResponsePriceException();

        int masterId = _currentUserService.GetUserId();

        Domain.Entities.Request? serviceRequest = await _dbContext.Requests
            .Include(request => request.Client)
            .FirstOrDefaultAsync(serviceRequest => serviceRequest.RequestId == requestId);

        if (serviceRequest is null)
            throw new RequestNotFoundException();

        string? requestStatus = await _dbContext.RequestStatuses
            .Where(status => status.RequestStatusId == serviceRequest.RequestStatusId)
            .Select(status => status.Name)
            .FirstOrDefaultAsync();

        if (requestStatus != OpenRequestStatusName)
            throw new RequestNotActiveException();

        bool requestAvailableForMaster = await _dbContext.MasterCategories
            .AnyAsync(masterCategory =>
                masterCategory.UserId == masterId &&
                masterCategory.CategoryId == serviceRequest.CategoryId);

        if (!requestAvailableForMaster)
            throw new RequestNotAvailableForMasterException();

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

        await _notificationClient.NotifyUserAsync(new NotificationInfoRequest
        {
            PhoneNumber = serviceRequest.Client.Phone,
            Message = ($"New response for your request {serviceRequest.Title} has arrived!")
        });

        return await GetResponseForRequestListItemAsync(response.ResponseId);
    }

    public async Task<IReadOnlyCollection<ResponseForRequestListItemResponse>> GetByRequestIdAsync(int requestId)
    {
        if (_currentUserService.GetRole() != ClientRoleName)
            throw new ResponseAccessDeniedException();

        int clientId = _currentUserService.GetUserId();

        Domain.Entities.Request? serviceRequest = await _dbContext.Requests
            .FirstOrDefaultAsync(serviceRequest => serviceRequest.RequestId == requestId);

        if (serviceRequest is null || serviceRequest.ClientId != clientId)
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
        if (_currentUserService.GetRole() != MasterRoleName)
            throw new ResponseAccessDeniedException();

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

    public async Task AcceptAsync(int responseId)
    {
        if (_currentUserService.GetRole() != ClientRoleName)
            throw new ResponseAccessDeniedException();

        int clientId = _currentUserService.GetUserId();

        Domain.Entities.Response? response = await _dbContext.Responses
            .Include(response => response.Request)
            .FirstOrDefaultAsync(response => response.ResponseId == responseId);

        if (response is null || response.Request.ClientId != clientId)
            throw new ResponseNotFoundException();

        int pendingStatusId = await GetResponseStatusIdAsync(PendingStatusName);
        int acceptedStatusId = await GetResponseStatusIdAsync(AcceptedStatusName);
        int rejectedStatusId = await GetResponseStatusIdAsync(RejectedStatusName);

        if (response.ResponseStatusId != pendingStatusId)
            throw new ResponseAlreadyProcessedException();

        response.ResponseStatusId = acceptedStatusId;

        List<Domain.Entities.Response> otherResponses = await _dbContext.Responses
            .Where(otherResponse =>
                otherResponse.RequestId == response.RequestId &&
                otherResponse.ResponseId != response.ResponseId &&
                otherResponse.ResponseStatusId == pendingStatusId)
            .ToListAsync();

        foreach (Domain.Entities.Response otherResponse in otherResponses)
        {
            otherResponse.ResponseStatusId = rejectedStatusId;
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task CancelAsync(int responseId)
    {
        if (_currentUserService.GetRole() != MasterRoleName)
            throw new ResponseAccessDeniedException();

        int masterId = _currentUserService.GetUserId();

        Domain.Entities.Response? response = await _dbContext.Responses
            .FirstOrDefaultAsync(response => response.ResponseId == responseId);

        if (response is null || response.MasterId != masterId)
            throw new ResponseNotFoundException();

        int pendingStatusId = await GetResponseStatusIdAsync(PendingStatusName);
        int cancelledStatusId = await GetResponseStatusIdAsync(CancelledStatusName);

        if (response.ResponseStatusId != pendingStatusId)
            throw new ResponseAlreadyProcessedException();

        response.ResponseStatusId = cancelledStatusId;

        await _dbContext.SaveChangesAsync();
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

    private async Task<int> GetResponseStatusIdAsync(string statusName)
    {
        int statusId = await _dbContext.ResponseStatuses
            .Where(status => status.Name == statusName)
            .Select(status => status.ResponseStatusId)
            .FirstOrDefaultAsync();

        if (statusId == 0)
            throw new ResponseStatusNotFoundException();

        return statusId;
    }
}
