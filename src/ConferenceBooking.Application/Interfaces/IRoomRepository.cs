using Ardalis.Specification;
using ConferenceBooking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Application.Interfaces;

public interface IRoomRepository
{
    Task<Room?> FirstOrDefaultAsync(ISpecification<Room> specification, CancellationToken cancellationToken = default);
    Task<List<Room>> ListAsync(ISpecification<Room> specification, CancellationToken cancellationToken = default);
    void Add(Room room);
    void Remove(Room room);
}
