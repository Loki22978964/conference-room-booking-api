using Ardalis.Specification.EntityFrameworkCore;
using ConferenceBooking.Application.Specifications;
using ConferenceBooking.Domain.Entities;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.UnitTests.Application.Specifications;

public class BookingsByDateRangeSpecTests : SpecificationTestBase
{
    [Fact]
    public async Task GetQuery_ReturnsOnlyBookingsWithinRange_AndIncludesRoom()
    {
        var room = new Room("Hall A", 10, 100m);

        var inRange = new Booking(
            room.Id,
            new DateTime(2026, 9, 20, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 20, 11, 0, 0, DateTimeKind.Utc),
            room.BaseHourlyRate,
            100m,
            Enumerable.Empty<(Guid, decimal)>());

        var outOfRange = new Booking(
            room.Id,
            new DateTime(2026, 9, 25, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 25, 11, 0, 0, DateTimeKind.Utc),
            room.BaseHourlyRate,
            100m,
            Enumerable.Empty<(Guid, decimal)>());

        room.Bookings.Add(inRange);
        room.Bookings.Add(outOfRange);

        using (var context = CreateContext())
        {
            context.Rooms.Add(room);
            await context.SaveChangesAsync();
        }

        using var queryContext = CreateContext();
        var spec = new BookingsByDateRangeSpec(
            new DateTime(2026, 9, 19, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 21, 0, 0, 0, DateTimeKind.Utc));

        var result = await SpecificationEvaluator.Default
            .GetQuery(queryContext.Bookings.AsQueryable(), spec)
            .ToListSafeAsync();

        result.Should().ContainSingle(b => b.Id == inRange.Id);
        result[0].Room.Should().NotBeNull();
        result[0].Room.Name.Should().Be("Hall A");
    }

    [Fact]
    public async Task GetQuery_WhenNoBookingsInRange_ReturnsEmpty()
    {
        var room = new Room("Hall B", 10, 100m);
        var booking = new Booking(
            room.Id,
            new DateTime(2026, 9, 25, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 25, 11, 0, 0, DateTimeKind.Utc),
            room.BaseHourlyRate,
            100m,
            Enumerable.Empty<(Guid, decimal)>());

        room.Bookings.Add(booking);

        using (var context = CreateContext())
        {
            context.Rooms.Add(room);
            await context.SaveChangesAsync();
        }

        using var queryContext = CreateContext();
        var spec = new BookingsByDateRangeSpec(
            new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 10, 0, 0, 0, DateTimeKind.Utc));

        var result = await SpecificationEvaluator.Default
            .GetQuery(queryContext.Bookings.AsQueryable(), spec)
            .ToListSafeAsync();

        result.Should().BeEmpty();
    }
}