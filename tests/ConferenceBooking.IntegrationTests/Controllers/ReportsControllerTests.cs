using ConferenceBooking.Application.DTOs;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace ConferenceBooking.IntegrationTests.Controllers;

[Collection("Integration Tests")]
public class ReportsControllerTests
{
    private readonly HttpClient _client;

    public ReportsControllerTests(IntegrationTestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    private record CreatedResponse(Guid Id);

    private async Task<Guid> CreateRoomAsync(string prefix, decimal baseHourlyRate)
    {
        var request = new CreateOrUpdateRoomRequest
        {
            Name = $"{prefix}-{Guid.NewGuid():N}",
            Capacity = 20,
            BaseHourlyRate = baseHourlyRate
        };

        var response = await _client.PostAsJsonAsync("/api/Rooms", request);
        var created = await response.Content.ReadFromJsonAsync<CreatedResponse>();
        return created!.Id;
    }

    [Fact]
    public async Task GetRevenueReportJson_IncludesRevenueFromBookingsWithinRange()
    {
        var roomId = await CreateRoomAsync("ReportHall", baseHourlyRate: 1000m);
        var bookingStart = DateTime.UtcNow.AddDays(3);

        await _client.PostAsJsonAsync("/api/Bookings", new CreateBookingRequest
        {
            RoomId = roomId,
            StartDateTimeUtc = bookingStart,
            DurationHours = 1,
            ServiceIds = new List<Guid>()
        });

        var periodStart = bookingStart.AddDays(-1).ToString("O");
        var periodEnd = bookingStart.AddDays(1).ToString("O");

        var response = await _client.GetAsync($"/api/Reports/revenue?startDate={periodStart}&endDate={periodEnd}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var report = await response.Content.ReadFromJsonAsync<RevenueReportDto>();

        report!.TotalOverallRevenue.Should().BeGreaterThan(0);
        report.RoomDetails.Should().Contain(r => r.RoomId == roomId);
    }

    [Fact]
    public async Task ExportRevenueReportCsv_ReturnsCsvFileWithCorrectContentType()
    {
        var periodStart = DateTime.UtcNow.AddDays(-30).ToString("O");
        var periodEnd = DateTime.UtcNow.AddDays(30).ToString("O");

        var response = await _client.GetAsync($"/api/Reports/revenue/export?startDate={periodStart}&endDate={periodEnd}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");

        var csv = await response.Content.ReadAsStringAsync();
        csv.Should().StartWith("Room ID,Room Name,Total Bookings,Total Revenue (UAH)");
    }
}
