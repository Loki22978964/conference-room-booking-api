using ConferenceBooking.Application.DTOs;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace ConferenceBooking.IntegrationTests.Controllers;

[Collection("Integration Tests")]
public class BookingsControllerTests
{
    private readonly HttpClient _client;

    public BookingsControllerTests(IntegrationTestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    private record CreatedResponse(Guid Id);
    private record BookingSuccessResponse(Guid BookingId, decimal TotalPrice, string Message);

    private async Task<Guid> CreateRoomAsync(string prefix)
    {
        var request = new CreateOrUpdateRoomRequest
        {
            Name = $"{prefix}-{Guid.NewGuid():N}",
            Capacity = 20,
            BaseHourlyRate = 500m
        };

        var response = await _client.PostAsJsonAsync("/api/Rooms", request);
        var created = await response.Content.ReadFromJsonAsync<CreatedResponse>();
        return created!.Id;
    }

    [Fact]
    public async Task BookRoom_WhenRoomExists_ReturnsOkWithCalculatedPrice()
    {
        var roomId = await CreateRoomAsync("BookableHall");

        var request = new CreateBookingRequest
        {
            RoomId = roomId,
            StartDateTimeUtc = DateTime.UtcNow.AddDays(5),
            DurationHours = 2,
            ServiceIds = new List<Guid>()
        };

        var response = await _client.PostAsJsonAsync("/api/Bookings", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<BookingSuccessResponse>();
        body!.BookingId.Should().NotBeEmpty();
        body.TotalPrice.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task BookRoom_WhenRoomDoesNotExist_ReturnsNotFound()
    {
        var request = new CreateBookingRequest
        {
            RoomId = Guid.NewGuid(),
            StartDateTimeUtc = DateTime.UtcNow.AddDays(5),
            DurationHours = 1,
            ServiceIds = new List<Guid>()
        };

        var response = await _client.PostAsJsonAsync("/api/Bookings", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task BookRoom_WhenSlotAlreadyBooked_ReturnsConflict()
    {
        // This tests the actual PostgreSQL exclusion constraint end-to-end —
        // something a unit test with an in-memory provider cannot possibly verify.
        var roomId = await CreateRoomAsync("OverlapHall");
        var start = DateTime.UtcNow.AddDays(7);

        var firstRequest = new CreateBookingRequest
        {
            RoomId = roomId,
            StartDateTimeUtc = start,
            DurationHours = 2,
            ServiceIds = new List<Guid>()
        };

        var firstResponse = await _client.PostAsJsonAsync("/api/Bookings", firstRequest);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var overlappingRequest = new CreateBookingRequest
        {
            RoomId = roomId,
            StartDateTimeUtc = start.AddHours(1), // overlaps with the first booking
            DurationHours = 2,
            ServiceIds = new List<Guid>()
        };

        var secondResponse = await _client.PostAsJsonAsync("/api/Bookings", overlappingRequest);

        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
