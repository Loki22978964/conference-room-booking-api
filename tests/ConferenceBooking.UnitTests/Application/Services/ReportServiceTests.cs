using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Application.Services;
using ConferenceBooking.Domain.Entities;
using ConferenceBooking.Infrastructure.Persistence;
using ConferenceBooking.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ConferenceBooking.UnitTests.Application.Services;

public class ReportServiceTests
{
    // Якір у майбутньому відносно моменту запуску тестів, щоб конструктор Booking
    // (який забороняє бронювання заднім числом) ніколи не блокував тестові дані.
    private static readonly DateTime Anchor = DateTime.UtcNow.Date.AddDays(30);

    private static AppDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new AppDbContext(options);
    }

    // Спрощений хелпер: hourlyRate == roomCost == totalPrice, без послуг,
    // щоб контролювати підсумкову суму напряму, не залежачи від PriceCalculator.
    // dayOffset відлічується від Anchor, а не від абсолютної календарної дати.
    private static Booking CreateBooking(Guid roomId, int dayOffset, decimal totalPrice)
    {
        var startUtc = Anchor.AddDays(dayOffset);
        return new Booking(roomId, startUtc, startUtc.AddHours(1), totalPrice, totalPrice,
            Enumerable.Empty<(Guid, decimal)>());
    }

    private static ReportService CreateSut(AppDbContext context)
    {
        var bookingRepository = new BookingRepository(context);
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Bookings).Returns(bookingRepository);

        return new ReportService(unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetRevenueReportAsync_GroupsAndSumsRevenuePerRoom_OrderedDescending()
    {
        var dbName = Guid.NewGuid().ToString();
        var roomA = new Room("Hall A", 10, 100m);
        var roomB = new Room("Hall B", 20, 200m);

        using (var seedContext = CreateContext(dbName))
        {
            seedContext.Rooms.AddRange(roomA, roomB);
            seedContext.Bookings.AddRange(
                CreateBooking(roomA.Id, dayOffset: 1, totalPrice: 100m),
                CreateBooking(roomA.Id, dayOffset: 2, totalPrice: 150m),
                CreateBooking(roomB.Id, dayOffset: 3, totalPrice: 500m));
            await seedContext.SaveChangesAsync();
        }

        using var queryContext = CreateContext(dbName);
        var sut = CreateSut(queryContext);

        var report = await sut.GetRevenueReportAsync(
            Anchor.AddDays(-1),
            Anchor.AddDays(10));

        report.RoomDetails.Should().HaveCount(2);
        report.TotalOverallRevenue.Should().Be(750m);

        report.RoomDetails[0].RoomName.Should().Be("Hall B"); // найбільша виручка - першою
        report.RoomDetails[0].TotalRevenue.Should().Be(500m);
        report.RoomDetails[0].TotalBookings.Should().Be(1);

        report.RoomDetails[1].RoomName.Should().Be("Hall A");
        report.RoomDetails[1].TotalRevenue.Should().Be(250m);
        report.RoomDetails[1].TotalBookings.Should().Be(2);
    }

    [Fact]
    public async Task GetRevenueReportAsync_ExcludesBookingsOutsideDateRange()
    {
        var dbName = Guid.NewGuid().ToString();
        var room = new Room("Hall C", 10, 100m);

        using (var seedContext = CreateContext(dbName))
        {
            seedContext.Rooms.Add(room);
            seedContext.Bookings.AddRange(
                CreateBooking(room.Id, dayOffset: 5, totalPrice: 100m),   // у межах
                CreateBooking(room.Id, dayOffset: 60, totalPrice: 999m)); // поза межами
            await seedContext.SaveChangesAsync();
        }

        using var queryContext = CreateContext(dbName);
        var sut = CreateSut(queryContext);

        var report = await sut.GetRevenueReportAsync(
            Anchor,
            Anchor.AddDays(10));

        report.RoomDetails.Should().ContainSingle();
        report.TotalOverallRevenue.Should().Be(100m);
    }

    [Fact]
    public async Task GetRevenueReportAsync_WhenNoBookingsInRange_ReturnsZeroTotalsAndEmptyDetails()
    {
        var dbName = Guid.NewGuid().ToString();

        using (var seedContext = CreateContext(dbName))
        {
            await seedContext.SaveChangesAsync(); // порожня база
        }

        using var queryContext = CreateContext(dbName);
        var sut = CreateSut(queryContext);

        var report = await sut.GetRevenueReportAsync(
            Anchor.AddDays(-100),
            Anchor.AddDays(100));

        report.RoomDetails.Should().BeEmpty();
        report.TotalOverallRevenue.Should().Be(0m);
    }

    [Fact]
    public async Task GetRevenueReportCsvAsync_ProducesExpectedCsvContent()
    {
        var dbName = Guid.NewGuid().ToString();
        var room = new Room("Hall D", 10, 100m);

        using (var seedContext = CreateContext(dbName))
        {
            seedContext.Rooms.Add(room);
            seedContext.Bookings.Add(CreateBooking(room.Id, dayOffset: 5, totalPrice: 250m));
            await seedContext.SaveChangesAsync();
        }

        using var queryContext = CreateContext(dbName);
        var sut = CreateSut(queryContext);

        var originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        try
        {
            var csvBytes = await sut.GetRevenueReportCsvAsync(
                Anchor,
                Anchor.AddDays(10));

            var csv = Encoding.UTF8.GetString(csvBytes);
            var lines = csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

            lines[0].Should().Be("Room ID,Room Name,Total Bookings,Total Revenue (UAH)");
            lines[1].Should().Be($"{room.Id},Hall D,1,250");
            lines[2].Should().Be(",,TOTAL:,250");
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}