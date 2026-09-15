using Ardalis.Specification;
using ConferenceBooking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Application.Specifications;

public class BookingsByDateRangeSpec : Specification<Booking>
{
    public BookingsByDateRangeSpec(DateTime startUtc, DateTime endUtc)
    {
        Query.Where(b => b.StartTimeUtc >= startUtc && b.StartTimeUtc <= endUtc)
             .Include(b => b.Room);
    }
}