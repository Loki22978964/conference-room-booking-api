using Ardalis.Specification;
using ConferenceBooking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Application.Interfaces;

public interface IBookingRepository
{
    Task<Booking?> FirstOrDefaultAsync(ISpecification<Booking> specification, CancellationToken cancellationToken = default);
    Task<List<Booking>> ListAsync(ISpecification<Booking> specification, CancellationToken cancellationToken = default);
    void Add(Booking booking);
    void Remove(Booking booking);
}