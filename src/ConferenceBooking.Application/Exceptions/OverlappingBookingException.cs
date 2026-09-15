using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Application.Exceptions;

public class OverlappingBookingException : Exception
{
    public OverlappingBookingException()
        : base("The room is already booked for the selected time slot.") { }
}
