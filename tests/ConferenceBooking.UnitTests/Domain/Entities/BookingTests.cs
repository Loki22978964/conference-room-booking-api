using ConferenceBooking.Domain.Entities;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.UnitTests.Domain.Entities;

public class BookingTests
{
    [Fact]
    public void Constructor_WhenStartTimeIsAfterEndTime_ThrowsArgumentException()
    {
        var roomId = Guid.NewGuid();
        var start = DateTime.UtcNow.AddHours(2);
        var end = DateTime.UtcNow.AddHours(1); // раніше за start

        var act = () => new Booking(roomId, start, end, 100m, 150m, Enumerable.Empty<(Guid, decimal)>());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenStartTimeIsInThePast_ThrowsArgumentException()
    {
        var roomId = Guid.NewGuid();
        var start = DateTime.UtcNow.AddHours(-1);
        var end = DateTime.UtcNow.AddHours(1);

        var act = () => new Booking(roomId, start, end, 100m, 150m, Enumerable.Empty<(Guid, decimal)>());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithSelectedServices_CalculatesTotalPriceCorrectly()
    {
        var roomId = Guid.NewGuid();
        var start = DateTime.UtcNow.AddHours(1);
        var end = start.AddHours(2);
        var services = new List<(Guid ServiceId, decimal Price)>
        {
            (Guid.NewGuid(), 500m),
            (Guid.NewGuid(), 300m)
        };

        var booking = new Booking(roomId, start, end, 200m, 400m, services);

        booking.TotalPrice.Should().Be(400m + 500m + 300m);
        booking.BookedServices.Should().HaveCount(2);
    }
}