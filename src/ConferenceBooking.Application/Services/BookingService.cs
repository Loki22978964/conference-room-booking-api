using ConferenceBooking.Application.DTOs;
using ConferenceBooking.Application.Exceptions;
using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Application.Specifications;
using ConferenceBooking.Domain.Entities;

namespace ConferenceBooking.Application.Services;

/// <summary>
/// Implements the room-booking use case: validating the room, calculating the
/// dynamic price, and persisting the booking while relying on the database's
/// exclusion constraint to guarantee no overlapping bookings are created.
/// </summary>
public class BookingService : IBookingService
{
    private readonly IUnitOfWork _unitOfWork;

    public BookingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Books a room for the requested time slot, optionally including add-on services,
    /// with the total price calculated dynamically based on time of day.
    /// </summary>
    /// <param name="request">The room, start time, duration, and selected service ids.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>
    /// <see cref="BookingResult.Success"/> with the new booking's id and total price;
    /// <see cref="BookingResult.RoomNotFound"/> if no active room exists with the given id;
    /// or <see cref="BookingResult.Overlapping"/> if the room is already booked for an
    /// overlapping time slot (detected via the PostgreSQL exclusion constraint on save).
    /// </returns>
    public async Task<BookingResult> BookRoomAsync(CreateBookingRequest request, CancellationToken cancellationToken = default)
    {
        var room = await _unitOfWork.Rooms.FirstOrDefaultAsync(
            new RoomByIdWithServicesSpec(request.RoomId), cancellationToken);

        if (room is null)
        {
            return new BookingResult.RoomNotFound();
        }

        var endTimeUtc = request.StartDateTimeUtc.AddHours(request.DurationHours);

        var selectedServices = room.RoomServices
            .Where(rs => request.ServiceIds.Contains(rs.ServiceId))
            .Select(rs => (rs.ServiceId, rs.Service.Price))
            .ToList();

        var calculatedRoomCost = PriceCalculator.CalculateRoomCost(
            request.StartDateTimeUtc,
            endTimeUtc,
            room.BaseHourlyRate);

        var booking = new Booking(
            room.Id,
            request.StartDateTimeUtc,
            endTimeUtc,
            room.BaseHourlyRate,
            calculatedRoomCost,
            selectedServices);

        _unitOfWork.Bookings.Add(booking);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (OverlappingBookingException)
        {
            // Thrown by the Infrastructure layer when the database's exclusion
            // constraint rejects an overlapping booking for the same room.
            return new BookingResult.Overlapping();
        }

        return new BookingResult.Success(booking.Id, booking.TotalPrice);
    }
}