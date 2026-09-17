using ConferenceBooking.Application.DTOs;
using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Application.Specifications;
using ConferenceBooking.Domain.Entities;

namespace ConferenceBooking.Application.Services;

/// <summary>
/// Implements room management use cases: creation, retrieval, updates,
/// soft-deletion, and availability search.
/// </summary>
public class RoomService : IRoomService
{
    private readonly IUnitOfWork _unitOfWork;

    public RoomService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Creates and persists a new room.
    /// </summary>
    /// <param name="request">The room's name, capacity, and base hourly rate.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The identifier of the newly created room.</returns>
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

    /// <summary>
    /// Retrieves a single room, including its associated services, by its identifier.
    /// </summary>
    /// <param name="id">The room's identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The mapped room DTO, or <see langword="null"/> if no active room exists with this id.</returns>
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

    /// <summary>
    /// Updates the name, capacity, and base hourly rate of an existing room.
    /// </summary>
    /// <param name="id">The identifier of the room to update.</param>
    /// <param name="request">The updated room details.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns><see langword="true"/> if the room was found and updated; otherwise <see langword="false"/>.</returns>
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

    /// <summary>
    /// Soft-deletes a room by marking it as inactive. The room is excluded from
    /// future queries (via the global query filter on <c>IsActive</c>) but its
    /// booking history is preserved.
    /// </summary>
    /// <param name="id">The identifier of the room to delete.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns><see langword="true"/> if the room was found and deleted; otherwise <see langword="false"/>.</returns>
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

    /// <summary>
    /// Searches for rooms that meet a minimum capacity and have no bookings
    /// overlapping the requested date and time window.
    /// </summary>
    /// <param name="request">The desired date, start/end time, and minimum capacity.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The list of matching, available rooms (may be empty).</returns>
    /// <remarks>
    /// <paramref name="request"/>'s date and time are combined and converted via
    /// <see cref="DateTime.ToUniversalTime"/>. If the combined value has
    /// <see cref="DateTimeKind.Unspecified"/>, it is treated as local server time —
    /// callers should ensure the intended time zone semantics match this behavior.
    /// </remarks>
    public async Task<List<RoomDto>> GetAvailableRoomsAsync(SearchRoomsRequest request, CancellationToken cancellationToken = default)
    {
        var startUtc = request.Date.Date.Add(request.StartTime).ToUniversalTime();
        var endUtc = request.Date.Date.Add(request.EndTime).ToUniversalTime();

        var spec = new AvailableRoomsSpec(startUtc, endUtc, request.Capacity);
        var rooms = await _unitOfWork.Rooms.ListAsync(spec, cancellationToken);

        return rooms.Select(r => new RoomDto
        {
            Id = r.Id,
            Name = r.Name,
            Capacity = r.Capacity,
            BaseHourlyRate = r.BaseHourlyRate,
            Services = r.RoomServices.Select(rs => new ServiceDto
            {
                Id = rs.Service.Id,
                Name = rs.Service.Name,
                Price = rs.Service.Price
            }).ToList()
        }).ToList();
    }
}