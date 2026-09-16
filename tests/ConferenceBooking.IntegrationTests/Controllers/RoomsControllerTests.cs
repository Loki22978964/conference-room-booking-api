using ConferenceBooking.Application.DTOs;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace ConferenceBooking.IntegrationTests.Controllers;

[Collection("Integration Tests")]
public class RoomsControllerTests
{
    private readonly HttpClient _client;

    public RoomsControllerTests(IntegrationTestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    private record CreatedResponse(Guid Id);

    private static CreateOrUpdateRoomRequest UniqueRoomRequest(string prefix = "Room") => new()
    {
        Name = $"{prefix}-{Guid.NewGuid():N}",
        Capacity = 20,
        BaseHourlyRate = 500m
    };

    [Fact]
    public async Task CreateRoom_ReturnsCreated_WithLocationHeader()
    {
        var response = await _client.PostAsJsonAsync("/api/Rooms", UniqueRoomRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var body = await response.Content.ReadFromJsonAsync<CreatedResponse>();
        body!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetRoom_WhenRoomExists_ReturnsOkWithRoomDetails()
    {
        var createRequest = UniqueRoomRequest("HallGet");
        var createResponse = await _client.PostAsJsonAsync("/api/Rooms", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        var response = await _client.GetAsync($"/api/Rooms/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<RoomDto>();
        dto!.Name.Should().Be(createRequest.Name);
        dto.Capacity.Should().Be(createRequest.Capacity);
    }

    [Fact]
    public async Task GetRoom_WhenRoomDoesNotExist_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/Rooms/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateRoom_WhenRoomExists_ReturnsOk_AndPersistsChanges()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/Rooms", UniqueRoomRequest("HallUpdate"));
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        var updateRequest = new CreateOrUpdateRoomRequest
        {
            Name = $"UpdatedHall-{Guid.NewGuid():N}",
            Capacity = 99,
            BaseHourlyRate = 1234m
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/Rooms/{created!.Id}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/api/Rooms/{created.Id}");
        var dto = await getResponse.Content.ReadFromJsonAsync<RoomDto>();

        dto!.Name.Should().Be(updateRequest.Name);
        dto.Capacity.Should().Be(99);
        dto.BaseHourlyRate.Should().Be(1234m);
    }

    [Fact]
    public async Task UpdateRoom_WhenRoomDoesNotExist_ReturnsNotFound()
    {
        var response = await _client.PutAsJsonAsync($"/api/Rooms/{Guid.NewGuid()}", UniqueRoomRequest());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteRoom_WhenRoomExists_ReturnsOk_AndSubsequentGetReturnsNotFound()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/Rooms", UniqueRoomRequest("HallDelete"));
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        var deleteResponse = await _client.DeleteAsync($"/api/Rooms/{created!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Soft deletion: the room is excluded from subsequent queries via a global query filter.
        var getResponse = await _client.GetAsync($"/api/Rooms/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAvailableRooms_ReturnsOnlyRoomsMeetingCapacity_WithNoOverlappingBookings()
    {
        var bigRoomRequest = UniqueRoomRequest("BigHall");
        bigRoomRequest.Capacity = 100;
        var smallRoomRequest = UniqueRoomRequest("SmallHall");
        smallRoomRequest.Capacity = 5;

        await _client.PostAsJsonAsync("/api/Rooms", bigRoomRequest);
        await _client.PostAsJsonAsync("/api/Rooms", smallRoomRequest);

        var date = DateTime.UtcNow.Date.AddDays(10);
        var query = $"/api/Rooms/available?Date={date:yyyy-MM-dd}&StartTime=10:00:00&EndTime=12:00:00&Capacity=50";

        var response = await _client.GetAsync(query);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var rooms = await response.Content.ReadFromJsonAsync<List<RoomDto>>();
        rooms.Should().Contain(r => r.Name == bigRoomRequest.Name);
        rooms.Should().NotContain(r => r.Name == smallRoomRequest.Name);
    }
}
