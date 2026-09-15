using ConferenceBooking.Application.DTOs;
using ConferenceBooking.Application.Exceptions;
using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Application.Specifications;
using ConferenceBooking.Domain.Entities;

namespace ConferenceBooking.Application.Services;

public class BookingService : IBookingService
{
    private readonly IUnitOfWork _unitOfWork;

    public BookingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

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
            return new BookingResult.Overlapping();
        }

        return new BookingResult.Success(booking.Id, booking.TotalPrice);
    }
}