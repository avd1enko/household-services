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

        bool categoryExists = await _dbContext.ServiceCategories
            .AnyAsync(category => category.CategoryId == request.CategoryId);

        if (!categoryExists)
            throw new CategoryNotFoundException();

        RequestStatus? openStatus = await _dbContext.RequestStatuses
            .FirstOrDefaultAsync(status => status.Name == OpenStatusName);

        if (openStatus is null)
            throw new InvalidOperationException("Open request status does not exist");

        int clientId = _currentUserService.GetUserId();

        Domain.Entities.Request entity = request.ToEntity(clientId, openStatus.RequestStatusId);

        _dbContext.Requests.Add(entity);
        await _dbContext.SaveChangesAsync();

        return entity.ToResponse();
    }

    public async Task<RequestResponse> GetByIdAsync(int requestId)
    {
        Domain.Entities.Request? entity = await _dbContext.Requests
            .AsNoTracking()
            .FirstOrDefaultAsync(request => request.RequestId == requestId);

        if (entity is null)
            throw new RequestNotFoundException();

        if (!await CanCurrentUserReadAsync(entity))
            throw new RequestAccessDeniedException();

        return entity.ToResponse();
    }

    public async Task<List<RequestResponse>> GetAllAsync(RequestFilterRequest filter)
    {
        IQueryable<Domain.Entities.Request> query = _dbContext.Requests
            .AsNoTracking();

        query = await ApplyCurrentUserAccessFilterAsync(query);

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

        List<Domain.Entities.Request> requests = await query
            .OrderByDescending(request => request.CreatedAt)
            .ToListAsync();

        return requests
            .Select(request => request.ToResponse())
            .ToList();
    }

    private async Task<IQueryable<Domain.Entities.Request>> ApplyCurrentUserAccessFilterAsync(
        IQueryable<Domain.Entities.Request> query)
    {
        int currentUserId = _currentUserService.GetUserId();
        string currentUserRole = _currentUserService.GetRole();

        if (currentUserRole == ClientRoleName)
        {
            return query.Where(request => request.ClientId == currentUserId);
        }

        if (currentUserRole == MasterRoleName)
        {
            List<int> masterCategoryIds = await _dbContext.MasterCategories
                .AsNoTracking()
                .Where(masterCategory => masterCategory.UserId == currentUserId)
                .Select(masterCategory => masterCategory.CategoryId)
                .ToListAsync();

            return query.Where(request => masterCategoryIds.Contains(request.CategoryId));
        }

        return query.Where(request => false);
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
}
