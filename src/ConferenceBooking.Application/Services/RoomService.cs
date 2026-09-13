using ConferenceBooking.Application.DTOs;
using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Application.Specifications;
using ConferenceBooking.Domain.Entities;

namespace ConferenceBooking.Application.Services;

public class RoomService : IRoomService
{
    private readonly IUnitOfWork _unitOfWork;

    public RoomService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreateRoomAsync(CreateOrUpdateRoomRequest request, CancellationToken cancellationToken = default)
    {
        var room = new Room(request.Name, request.Capacity, request.BaseHourlyRate);

        // Note: It would be better to move the logic for adding services (RoomServices)
        // to a Domain Service or a separate method in the services repository,
        // but we are keeping the basic flow for the sake of simplicity.

        _unitOfWork.Rooms.Add(room);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return room.Id;
    }

    public async Task<RoomDto?> GetRoomAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var room = await _unitOfWork.Rooms.FirstOrDefaultAsync(new RoomByIdWithServicesSpec(id), cancellationToken);

        if (room is null)
        {
            return null;
        }

        return new RoomDto
        {
            Id = room.Id,
            Name = room.Name,
            Capacity = room.Capacity,
            BaseHourlyRate = room.BaseHourlyRate,
            Services = room.RoomServices.Select(rs => new ServiceDto
            {
                Id = rs.Service.Id,
                Name = rs.Service.Name,
                Price = rs.Service.Price
            }).ToList()
        };
    }

    public async Task<bool> UpdateRoomAsync(Guid id, CreateOrUpdateRoomRequest request, CancellationToken cancellationToken = default)
    {
        var room = await _unitOfWork.Rooms.FirstOrDefaultAsync(new RoomByIdWithServicesSpec(id), cancellationToken);

        if (room is null)
        {
            return false;
        }

        room.UpdateDetails(request.Name, request.Capacity, request.BaseHourlyRate);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteRoomAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var room = await _unitOfWork.Rooms.FirstOrDefaultAsync(new RoomByIdSpec(id), cancellationToken);

        if (room is null)
        {
            return false;
        }

        room.MarkAsDeleted();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}