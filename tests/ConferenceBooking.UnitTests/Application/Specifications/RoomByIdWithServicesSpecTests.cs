using Ardalis.Specification.EntityFrameworkCore;
using ConferenceBooking.Application.Specifications;
using ConferenceBooking.Domain.Entities;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.UnitTests.Application.Specifications;

public class RoomByIdWithServicesSpecTests : SpecificationTestBase
{
    [Fact]
    public async Task GetQuery_IncludesRoomServicesAndTheirService()
    {
        var service = new Service("Projector", 500m);
        var room = new Room("Hall A", 10, 100m);
        room.RoomServices.Add(new RoomService(room.Id, service.Id));

        using (var context = CreateContext())
        {
            context.Services.Add(service);
            context.Rooms.Add(room);
            await context.SaveChangesAsync();
        }

        using var queryContext = CreateContext();
        var spec = new RoomByIdWithServicesSpec(room.Id);

        var result = await SpecificationEvaluator.Default
            .GetQuery(queryContext.Rooms.AsQueryable(), spec)
            .FirstOrDefaultSafeAsync();

        result.Should().NotBeNull();
        result!.RoomServices.Should().ContainSingle();
        result.RoomServices.First().Service.Should().NotBeNull();
        result.RoomServices.First().Service.Name.Should().Be("Projector");
    }

    [Fact]
    public async Task GetQuery_WhenRoomHasNoServices_ReturnsEmptyCollection()
    {
        var room = new Room("Hall B", 10, 100m);

        using (var context = CreateContext())
        {
            context.Rooms.Add(room);
            await context.SaveChangesAsync();
        }

        using var queryContext = CreateContext();
        var spec = new RoomByIdWithServicesSpec(room.Id);

        var result = await SpecificationEvaluator.Default
            .GetQuery(queryContext.Rooms.AsQueryable(), spec)
            .FirstOrDefaultSafeAsync();

        result.Should().NotBeNull();
        result!.RoomServices.Should().BeEmpty();
    }
}
