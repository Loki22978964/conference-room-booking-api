using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Application.DTOs;

public class CreateBookingRequest
{
    public Guid RoomId { get; set; }
    public DateTime StartDateTimeUtc { get; set; }
    public int DurationHours { get; set; }
    public List<Guid> ServiceIds { get; set; } = new();
}
