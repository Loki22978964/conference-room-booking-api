using Ardalis.Specification.EntityFrameworkCore;
using ConferenceBooking.Application.Specifications;
using ConferenceBooking.Domain.Entities;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.UnitTests.Application.Specifications;

public class RoomByIdSpecTests : SpecificationTestBase
{
    [Fact]
    public async Task GetQuery_WhenIdMatches_ReturnsRoom()
    {
        var room = new Room("Hall A", 10, 100m);

        using (var context = CreateContext())
        {
            context.Rooms.Add(room);
            await context.SaveChangesAsync();
        }

        using var queryContext = CreateContext();
        var spec = new RoomByIdSpec(room.Id);

        var result = await SpecificationEvaluator.Default
            .GetQuery(queryContext.Rooms.AsQueryable(), spec)
            .FirstOrDefaultSafeAsync();

        result.Should().NotBeNull();
        result!.Id.Should().Be(room.Id);
    }

    [Fact]
    public async Task GetQuery_WhenIdDoesNotMatch_ReturnsNull()
    {
        var room = new Room("Hall B", 10, 100m);

        using (var context = CreateContext())
        {
            context.Rooms.Add(room);
            await context.SaveChangesAsync();
        }

        using var queryContext = CreateContext();
        var spec = new RoomByIdSpec(Guid.NewGuid());

        var result = await SpecificationEvaluator.Default
            .GetQuery(queryContext.Rooms.AsQueryable(), spec)
            .FirstOrDefaultSafeAsync();

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetQuery_WhenRoomIsSoftDeleted_ExcludedByGlobalQueryFilter()
    {
        var room = new Room("Hall C", 10, 100m);
        room.MarkAsDeleted();

        using (var context = CreateContext())
        {
            context.Rooms.Add(room);
            await context.SaveChangesAsync();
        }

        using var queryContext = CreateContext();
        var spec = new RoomByIdSpec(room.Id);

        var result = await SpecificationEvaluator.Default
            .GetQuery(queryContext.Rooms.AsQueryable(), spec)
            .FirstOrDefaultSafeAsync();

        result.Should().BeNull();
    }
}
