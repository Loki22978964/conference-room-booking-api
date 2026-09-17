using ConferenceBooking.Application.DTOs;
using ConferenceBooking.Domain.Entities;
using ConferenceBooking.Infrastructure.Persistence;
using ConferenceBooking.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.UnitTests.Infrastructure.Persistence;

public class DatabaseSeederTests
{
    private static AppDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task SeedAsync_WithInMemoryProvider_SkipsMigrations_AndSeedsData()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());

        // InMemory не є relational, тому IsRelational() == false,
        // і MigrateAsync() не викликається — виклик не повинен впасти.
        var act = async () => await DatabaseSeeder.SeedAsync(context);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SeedAsync_WhenDatabaseIsEmpty_CreatesThreeRoomsWithExpectedDetails()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());

        await DatabaseSeeder.SeedAsync(context);

        var rooms = await context.Rooms.ToListAsync();
        rooms.Should().HaveCount(3);

        rooms.Should().ContainSingle(r => r.Name == "Hall A" && r.Capacity == 50 && r.BaseHourlyRate == 2000m);
        rooms.Should().ContainSingle(r => r.Name == "Hall B" && r.Capacity == 100 && r.BaseHourlyRate == 3500m);
        rooms.Should().ContainSingle(r => r.Name == "Hall C" && r.Capacity == 30 && r.BaseHourlyRate == 1500m);
    }

    [Fact]
    public async Task SeedAsync_WhenDatabaseIsEmpty_CreatesThreeServicesWithExpectedDetails()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());

        await DatabaseSeeder.SeedAsync(context);

        var services = await context.Services.ToListAsync();
        services.Should().HaveCount(3);

        services.Should().ContainSingle(s => s.Name == "Projector" && s.Price == 500m);
        services.Should().ContainSingle(s => s.Name == "Wi-Fi" && s.Price == 300m);
        services.Should().ContainSingle(s => s.Name == "Sound" && s.Price == 700m);
    }

    [Fact]
    public async Task SeedAsync_WhenDatabaseIsEmpty_LinksEveryRoomToEveryService()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());

        await DatabaseSeeder.SeedAsync(context);

        var roomIds = await context.Rooms.Select(r => r.Id).ToListAsync();
        var serviceIds = await context.Services.Select(s => s.Id).ToListAsync();
        var links = await context.RoomServices.ToListAsync();

        // 3 зали × 3 послуги = 9 зв'язків (декартів добуток)
        links.Should().HaveCount(9);

        foreach (var roomId in roomIds)
        {
            foreach (var serviceId in serviceIds)
            {
                links.Should().ContainSingle(l => l.RoomId == roomId && l.ServiceId == serviceId);
            }
        }
    }

    [Fact]
    public async Task SeedAsync_WhenRoomsAlreadyExist_DoesNotAddMoreData()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());

        var existingRoom = new Room("Existing Hall", 10, 100m);
        context.Rooms.Add(existingRoom);
        await context.SaveChangesAsync();

        await DatabaseSeeder.SeedAsync(context);

        var rooms = await context.Rooms.ToListAsync();
        var services = await context.Services.ToListAsync();
        var links = await context.RoomServices.ToListAsync();

        rooms.Should().ContainSingle();
        rooms[0].Id.Should().Be(existingRoom.Id);
        services.Should().BeEmpty();
        links.Should().BeEmpty();
    }

    [Fact]
    public async Task SeedAsync_CalledTwiceInARow_IsIdempotent()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());

        await DatabaseSeeder.SeedAsync(context);
        var roomCountAfterFirstCall = await context.Rooms.CountAsync();

        await DatabaseSeeder.SeedAsync(context);
        var roomCountAfterSecondCall = await context.Rooms.CountAsync();

        roomCountAfterSecondCall.Should().Be(roomCountAfterFirstCall);
    }
}