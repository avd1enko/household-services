using HouseholdServices.Application.DTOs.Request;
using HouseholdServices.Application.Exceptions.Request;
using HouseholdServices.Application.Mappers;
using HouseholdServices.Application.Services.Request;
using HouseholdServices.Application.Services.Users;
using HouseholdServices.Domain.Entities;
using HouseholdServices.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HouseholdServices.Infrastructure.Services.Request;

public class RequestService : IRequestService
{
    private const string OpenStatusName = "open";
    private const string CancelledStatusName = "cancelled";
    private const string ClientRoleName = "client";
    private const string MasterRoleName = "master";

    private readonly HouseholdServicesDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public RequestService(HouseholdServicesDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<RequestResponse> CreateAsync(CreateRequestRequest request)
    {
        string currentUserRole = _currentUserService.GetRole();

        if (currentUserRole != ClientRoleName)
            throw new ClientRoleRequiredException();

        ServiceCategory? category = await _dbContext.ServiceCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(category => category.CategoryId == request.CategoryId);

        if (category is null)
            throw new CategoryNotFoundException();

        RequestStatus? openStatus = await _dbContext.RequestStatuses
            .FirstOrDefaultAsync(status => status.Name == OpenStatusName);

        if (openStatus is null)
            throw new InvalidOperationException("Open request status does not exist");

        int clientId = _currentUserService.GetUserId();

        Domain.Entities.Request entity = request.ToEntity(clientId, openStatus.RequestStatusId);

        _dbContext.Requests.Add(entity);
        await _dbContext.SaveChangesAsync();

        (string clientFirstName, string clientLastName) = await GetUserNameAsync(clientId);

        return entity.ToResponse(clientFirstName, clientLastName, category.Name, OpenStatusName);
    }

    public async Task<RequestResponse> GetByIdAsync(int requestId)
    {
        Domain.Entities.Request? entity = await _dbContext.Requests
            .AsNoTracking()
            .Include(request => request.Client)
            .FirstOrDefaultAsync(request => request.RequestId == requestId);

        if (entity is null)
            throw new RequestNotFoundException();

        if (!await CanCurrentUserReadAsync(entity))
            throw new RequestAccessDeniedException();

        var requestInfo = await _dbContext.Requests
            .AsNoTracking()
            .Where(request => request.RequestId == requestId)
            .Join(
                _dbContext.ServiceCategories.AsNoTracking(),
                request => request.CategoryId,
                category => category.CategoryId,
                (request, category) => new
                {
                    Request = request,
                    CategoryName = category.Name
                })
            .Join(
                _dbContext.RequestStatuses.AsNoTracking(),
                requestInfo => requestInfo.Request.RequestStatusId,
                status => status.RequestStatusId,
                (requestInfo, status) => new
                {
                    requestInfo.CategoryName,
                    Status = status.Name
                })
            .FirstAsync();

        return entity.ToResponse(
            entity.Client.FirstName,
            entity.Client.LastName,
            requestInfo.CategoryName,
            requestInfo.Status);
    }

    public async Task<List<AvailableRequestListItemResponse>> GetAvailableForCurrentMasterAsync(
        RequestFilterRequest filter)
    {
        string currentUserRole = _currentUserService.GetRole();

        if (currentUserRole != MasterRoleName)
            throw new RequestAccessDeniedException();

        int currentUserId = _currentUserService.GetUserId();

        RequestStatus? openStatus = await _dbContext.RequestStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(status => status.Name == OpenStatusName);

        if (openStatus is null)
            throw new InvalidOperationException("Open request status does not exist");

        List<int> masterCategoryIds = await _dbContext.MasterCategories
            .AsNoTracking()
            .Where(masterCategory => masterCategory.UserId == currentUserId)
            .Select(masterCategory => masterCategory.CategoryId)
            .ToListAsync();

        IQueryable<Domain.Entities.Request> query = _dbContext.Requests
            .AsNoTracking()
            .Where(request =>
                request.RequestStatusId == openStatus.RequestStatusId &&
                masterCategoryIds.Contains(request.CategoryId));

        query = ApplyRequestFilters(query, filter);

        var requests = await query
            .Join(
                _dbContext.Users.AsNoTracking(),
                request => request.ClientId,
                user => user.UserId,
                (request, user) => new
                {
                    Request = request,
                    ClientFirstName = user.FirstName,
                    ClientLastName = user.LastName
                })
            .Join(
                _dbContext.ServiceCategories.AsNoTracking(),
                requestInfo => requestInfo.Request.CategoryId,
                category => category.CategoryId,
                (requestInfo, category) => new
                {
                    requestInfo.Request,
                    requestInfo.ClientFirstName,
                    requestInfo.ClientLastName,
                    CategoryName = category.Name
                })
            .OrderByDescending(requestInfo => requestInfo.Request.CreatedAt)
            .ToListAsync();

        return requests
            .Select(requestInfo => requestInfo.Request.ToAvailableListItem(
                requestInfo.ClientFirstName,
                requestInfo.ClientLastName,
                requestInfo.CategoryName,
                OpenStatusName))
            .ToList();
    }

    public async Task<List<UserRequestListItemResponse>> GetCurrentUserRequestsAsync(RequestFilterRequest filter)
    {
        string currentUserRole = _currentUserService.GetRole();

        if (currentUserRole != ClientRoleName)
            throw new RequestAccessDeniedException();

        int currentUserId = _currentUserService.GetUserId();

        IQueryable<Domain.Entities.Request> query = _dbContext.Requests
            .AsNoTracking()
            .Where(request => request.ClientId == currentUserId);

        query = ApplyRequestFilters(query, filter);

        var requests = await query
            .Join(
                _dbContext.ServiceCategories.AsNoTracking(),
                request => request.CategoryId,
                category => category.CategoryId,
                (request, category) => new
                {
                    Request = request,
                    CategoryName = category.Name
                })
            .Join(
                _dbContext.RequestStatuses.AsNoTracking(),
                requestInfo => requestInfo.Request.RequestStatusId,
                status => status.RequestStatusId,
                (requestInfo, status) => new
                {
                    requestInfo.Request,
                    requestInfo.CategoryName,
                    Status = status.Name
                })
            .OrderByDescending(requestInfo => requestInfo.Request.CreatedAt)
            .ToListAsync();

        return requests
            .Select(requestInfo => requestInfo.Request.ToUserListItem(requestInfo.CategoryName, requestInfo.Status))
            .ToList();
    }

    public async Task CancelAsync(int requestId)
    {
        string currentUserRole = _currentUserService.GetRole();

        if (currentUserRole != ClientRoleName)
            throw new RequestAccessDeniedException();

        int currentUserId = _currentUserService.GetUserId();

        Domain.Entities.Request? request = await _dbContext.Requests
            .FirstOrDefaultAsync(request => request.RequestId == requestId);

        if (request is null)
            throw new RequestNotFoundException();

        if (request.ClientId != currentUserId)
            throw new RequestAccessDeniedException();

        RequestStatus? openStatus = await _dbContext.RequestStatuses
            .FirstOrDefaultAsync(status => status.Name == OpenStatusName);

        if (openStatus is null)
            throw new InvalidOperationException("Open request status does not exist");

        if (request.RequestStatusId != openStatus.RequestStatusId)
            throw new RequestCannotBeCancelledException();

        RequestStatus? cancelledStatus = await _dbContext.RequestStatuses
            .FirstOrDefaultAsync(status => status.Name == CancelledStatusName);

        if (cancelledStatus is null)
            throw new InvalidOperationException("Cancelled request status does not exist");

        request.RequestStatusId = cancelledStatus.RequestStatusId;
        await _dbContext.SaveChangesAsync();
    }

    private static IQueryable<Domain.Entities.Request> ApplyRequestFilters(
        IQueryable<Domain.Entities.Request> query,
        RequestFilterRequest filter)
    {

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(request => request.CategoryId == filter.CategoryId.Value);
        }

        if (filter.DesiredDateFrom.HasValue)
        {
            query = query.Where(request => request.DesiredDate >= filter.DesiredDateFrom.Value);
        }

        if (filter.DesiredDateTo.HasValue)
        {
            query = query.Where(request => request.DesiredDate <= filter.DesiredDateTo.Value);
        }

        if (filter.CreatedAtFrom.HasValue)
        {
            query = query.Where(request => request.CreatedAt >= filter.CreatedAtFrom.Value);
        }

        if (filter.CreatedAtTo.HasValue)
        {
            query = query.Where(request => request.CreatedAt <= filter.CreatedAtTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Title))
        {
            string title = filter.Title.Trim().ToLower();
            query = query.Where(request => request.Title.ToLower().Contains(title));
        }

        return query;
    }

    private async Task<bool> CanCurrentUserReadAsync(Domain.Entities.Request request)
    {
        int currentUserId = _currentUserService.GetUserId();
        string currentUserRole = _currentUserService.GetRole();

        if (currentUserRole == ClientRoleName)
        {
            return request.ClientId == currentUserId;
        }

        if (currentUserRole == MasterRoleName)
        {
            return await _dbContext.MasterCategories
                .AsNoTracking()
                .AnyAsync(masterCategory =>
                    masterCategory.UserId == currentUserId &&
                    masterCategory.CategoryId == request.CategoryId);
        }

        return false;
    }

    private async Task<(string FirstName, string LastName)> GetUserNameAsync(int userId)
    {
        var userName = await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.UserId == userId)
            .Select(user => new
            {
                user.FirstName,
                user.LastName
            })
            .FirstOrDefaultAsync();

        if (userName is null)
            throw new InvalidOperationException("Current user does not exist");

        return (userName.FirstName, userName.LastName);
    }
}
