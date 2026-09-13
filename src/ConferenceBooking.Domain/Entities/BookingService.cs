using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Domain.Entities;

public class BookingService
{
    public Guid BookingId { get; private set; }
    public Booking Booking { get; private set; } = null!;

    public Guid ServiceId { get; private set; }
    public Service Service { get; private set; } = null!;

    public decimal PriceSnapshot { get; private set; }

    private BookingService() { }

    public BookingService(Guid bookingId, Guid serviceId, decimal priceSnapshot)
    {
        BookingId = bookingId;
        ServiceId = serviceId;
        PriceSnapshot = priceSnapshot;
    }
}
