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

    public static RequestResponse ToResponse(this Domain.Entities.Request request)
    {
        return new RequestResponse
        {
            RequestId = request.RequestId,
            ClientId = request.ClientId,
            CategoryId = request.CategoryId,
            RequestStatusId = request.RequestStatusId,
            Title = request.Title,
            Description = request.Description,
            Address = request.Address,
            DesiredDate = request.DesiredDate,
            CreatedAt = request.CreatedAt
        };
    }
}
