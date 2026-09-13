using ConferenceBooking.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Application.Interfaces;

public interface IRoomService
{
    Task<Guid> CreateRoomAsync(CreateOrUpdateRoomRequest request, CancellationToken cancellationToken = default);
    Task<RoomDto?> GetRoomAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> UpdateRoomAsync(Guid id, CreateOrUpdateRoomRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteRoomAsync(Guid id, CancellationToken cancellationToken = default);
}