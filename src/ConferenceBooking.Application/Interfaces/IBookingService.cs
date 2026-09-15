using ConferenceBooking.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Application.Interfaces;

public interface IBookingService
{
    Task<BookingResult> BookRoomAsync(CreateBookingRequest request, CancellationToken cancellationToken = default);
}

public abstract record BookingResult
{
    public sealed record Success(Guid BookingId, decimal TotalPrice) : BookingResult;
    public sealed record RoomNotFound : BookingResult;
    public sealed record Overlapping : BookingResult;
}