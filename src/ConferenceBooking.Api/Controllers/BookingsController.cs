using ConferenceBooking.Application.DTOs;
using ConferenceBooking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

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