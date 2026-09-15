using ConferenceBooking.Application.DTOs;
using ConferenceBooking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceBooking.Api.Controllers;

/// <summary>
/// Handles the creation of conference room bookings.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    /// Books a room for a given time slot, optionally including add-on services.
    /// The total price is calculated dynamically based on the time of day.
    /// </summary>
    /// <param name="request">The room, start time, duration, and selected service ids.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>
    /// The created booking's id and total price on success; a 404 response if the room
    /// does not exist; or a 409 response if the room is already booked for the requested slot.
    /// </returns>
    /// <response code="200">The room was booked successfully.</response>
    /// <response code="404">No room exists with the given id.</response>
    /// <response code="409">The room is already booked for an overlapping time slot.</response>
    [HttpPost]
    public async Task<IActionResult> BookRoom([FromBody] CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var result = await _bookingService.BookRoomAsync(request, cancellationToken);

        return result switch
        {
            BookingResult.Success s => Ok(new { BookingId = s.BookingId, TotalPrice = s.TotalPrice, Message = "Room booked successfully." }),
            BookingResult.RoomNotFound => NotFound("Room not found."),
            BookingResult.Overlapping => Conflict(new { Message = "The room is already booked for the selected time slot." }),
            _ => Problem("Unexpected result.")
        };
    }
}