using ConferenceBooking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Application.Interfaces;

public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Room?> GetByIdWithServicesAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Room room);
    void Remove(Room room);
}
