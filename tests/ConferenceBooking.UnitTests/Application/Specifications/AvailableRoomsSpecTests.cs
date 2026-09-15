using Ardalis.Specification.EntityFrameworkCore;
using ConferenceBooking.Application.Specifications;
using ConferenceBooking.Domain.Entities;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.UnitTests.Application.Specifications;

public class AvailableRoomsSpecTests : SpecificationTestBase
{
    private static readonly DateTime SearchStart = new(2026, 9, 20, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime SearchEnd = new(2026, 9, 20, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task GetQuery_ExcludesRoomsBelowRequestedCapacity()
    {
        var smallRoom = new Room("Small", 5, 100m);
        var bigRoom = new Room("Big", 50, 200m);

        using (var context = CreateContext())
        {
            context.Rooms.AddRange(smallRoom, bigRoom);
            await context.SaveChangesAsync();
        }

        using var queryContext = CreateContext();
        var spec = new AvailableRoomsSpec(SearchStart, SearchEnd, capacity: 10);

        var result = await SpecificationEvaluator.Default
            .GetQuery(queryContext.Rooms.AsQueryable(), spec)
            .ToListSafeAsync();

        result.Should().ContainSingle(r => r.Id == bigRoom.Id);
    }

    [Fact]
    public async Task GetQuery_ExcludesRoomWithOverlappingBooking()
    {
        var room = new Room("Hall A", 10, 100m);

        // 11:00–13:00 перетинається з вікном пошуку 10:00–12:00
        var overlapping = new Booking(
            room.Id,
            new DateTime(2026, 9, 20, 11, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 20, 13, 0, 0, DateTimeKind.Utc),
            room.BaseHourlyRate,
            200m,
            Enumerable.Empty<(Guid, decimal)>());

        room.Bookings.Add(overlapping);

        using (var context = CreateContext())
        {
            context.Rooms.Add(room);
            await context.SaveChangesAsync();
        }

        using var queryContext = CreateContext();
        var spec = new AvailableRoomsSpec(SearchStart, SearchEnd, capacity: 5);

        var result = await SpecificationEvaluator.Default
            .GetQuery(queryContext.Rooms.AsQueryable(), spec)
            .ToListSafeAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetQuery_IncludesRoomWithNonOverlappingBooking()
    {
        var room = new Room("Hall B", 10, 100m);

        // 13:00–14:00 — після завершення вікна пошуку (10:00–12:00)
        var nonOverlapping = new Booking(
            room.Id,
            new DateTime(2026, 9, 20, 13, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 20, 14, 0, 0, DateTimeKind.Utc),
            room.BaseHourlyRate,
            100m,
            Enumerable.Empty<(Guid, decimal)>());

        room.Bookings.Add(nonOverlapping);

        using (var context = CreateContext())
        {
            context.Rooms.Add(room);
            await context.SaveChangesAsync();
        }

        using var queryContext = CreateContext();
        var spec = new AvailableRoomsSpec(SearchStart, SearchEnd, capacity: 5);

        var result = await SpecificationEvaluator.Default
            .GetQuery(queryContext.Rooms.AsQueryable(), spec)
            .ToListSafeAsync();

        result.Should().ContainSingle(r => r.Id == room.Id);
    }

    [Fact]
    public async Task GetQuery_IncludesRoomServicesAndTheirService()
    {
        var service = new Service("Wi-Fi", 50m);
        var room = new Room("Hall C", 10, 100m);
        room.RoomServices.Add(new RoomService(room.Id, service.Id));

        using (var context = CreateContext())
        {
            context.Services.Add(service);
            context.Rooms.Add(room);
            await context.SaveChangesAsync();
        }

        using var queryContext = CreateContext();
        var spec = new AvailableRoomsSpec(SearchStart, SearchEnd, capacity: 5);

        var result = await SpecificationEvaluator.Default
            .GetQuery(queryContext.Rooms.AsQueryable(), spec)
            .ToListSafeAsync();

        result.Should().ContainSingle();
        result[0].RoomServices.Should().ContainSingle(rs => rs.Service.Name == "Wi-Fi");
    }
}
