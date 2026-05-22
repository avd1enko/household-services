using HouseholdServices.Application.DTOs.Request;

namespace HouseholdServices.Application.Mappers;

public static class RequestMapper
{
    public static Domain.Entities.Request ToEntity(
        this CreateRequestRequest request,
        int clientId,
        int requestStatusId)
    {
        return new Domain.Entities.Request
        {
            ClientId = clientId,
            CategoryId = request.CategoryId,
            RequestStatusId = requestStatusId,
            Title = request.Title,
            Description = request.Description,
            Address = request.Address,
            DesiredDate = request.DesiredDate,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static RequestResponse ToResponse(this Domain.Entities.Request request, string clientFirstName)
    {
        return new RequestResponse
        {
            RequestId = request.RequestId,
            ClientId = request.ClientId,
            ClientFirstName = clientFirstName,
            CategoryId = request.CategoryId,
            RequestStatusId = request.RequestStatusId,
            Title = request.Title,
            Description = request.Description,
            Address = request.Address,
            DesiredDate = request.DesiredDate,
            CreatedAt = request.CreatedAt
        };
    }

    public static UserRequestListItemResponse ToUserListItem(
        this Domain.Entities.Request request,
        string categoryName)
    {
        return new UserRequestListItemResponse
        {
            RequestId = request.RequestId,
            CategoryId = request.CategoryId,
            CategoryName = categoryName,
            DesiredDate = request.DesiredDate,
            CreatedAt = request.CreatedAt
        };
    }

    public static AvailableRequestListItemResponse ToAvailableListItem(
        this Domain.Entities.Request request,
        string clientFirstName,
        string categoryName,
        string status)
    {
        return new AvailableRequestListItemResponse
        {
            RequestId = request.RequestId,
            ClientId = request.ClientId,
            ClientFirstName = clientFirstName,
            CategoryId = request.CategoryId,
            CategoryName = categoryName,
            Title = request.Title,
            Description = request.Description,
            Address = request.Address,
            DesiredDate = request.DesiredDate,
            CreatedAt = request.CreatedAt,
            Status = status
        };
    }
}
