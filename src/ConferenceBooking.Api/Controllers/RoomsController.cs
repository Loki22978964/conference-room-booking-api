using ConferenceBooking.Application.DTOs;
using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Domain.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceBooking.Api.Controllers;

/// <summary>
/// Manages conference rooms: creation, retrieval, updates, soft-deletion,
/// and availability search.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    /// <summary>
    /// Creates a new conference room.
    /// </summary>
    /// <param name="request">The room's name, capacity, and base hourly rate.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The identifier of the newly created room.</returns>
    /// <response code="201">The room was created successfully.</response>
    [HttpPost]
    public async Task<IActionResult> CreateRoom([FromBody] CreateOrUpdateRoomRequest request, CancellationToken cancellationToken)
    {
        var roomId = await _roomService.CreateRoomAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetRoom), new { id = roomId }, new { Id = roomId });
    }

    /// <summary>
    /// Retrieves a single room by its identifier, including its associated services.
    /// </summary>
    /// <param name="id">The room's identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The room details, or a 404 response if no active room with this id exists.</returns>
    /// <response code="200">The room was found and returned.</response>
    /// <response code="404">No active room exists with the given id.</response>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoom(Guid id, CancellationToken cancellationToken)
    {
        var dto = await _roomService.GetRoomAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    /// <summary>
    /// Updates the name, capacity, and base hourly rate of an existing room.
    /// </summary>
    /// <param name="id">The identifier of the room to update.</param>
    /// <param name="request">The updated room details.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A confirmation message, or a 404 response if the room does not exist.</returns>
    /// <response code="200">The room was updated successfully.</response>
    /// <response code="404">No room exists with the given id.</response>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoom(Guid id, [FromBody] CreateOrUpdateRoomRequest request, CancellationToken cancellationToken)
    {
        var updated = await _roomService.UpdateRoomAsync(id, request, cancellationToken);
        return updated ? Ok(new { Message = "Room updated successfully" }) : NotFound("Room not found");
    }

    /// <summary>
    /// Soft-deletes a room by marking it as inactive. The room is excluded
    /// from future queries but its booking history is preserved.
    /// </summary>
    /// <param name="id">The identifier of the room to delete.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A confirmation message, or a 404 response if the room does not exist.</returns>
    /// <response code="200">The room was deleted successfully.</response>
    /// <response code="404">No room exists with the given id.</response>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoom(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _roomService.DeleteRoomAsync(id, cancellationToken);
        return deleted ? Ok(new { Message = "Room deleted successfully" }) : NotFound();
    }

    /// <summary>
    /// Searches for rooms that are available for a given date, time window,
    /// and minimum capacity.
    /// </summary>
    /// <param name="request">The desired date, start/end time, and minimum capacity.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A list of rooms that satisfy the capacity requirement and have no overlapping bookings.</returns>
    /// <response code="200">The list of available rooms was returned (may be empty).</response>
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableRooms([FromQuery] SearchRoomsRequest request, CancellationToken cancellationToken)
    {
        var result = await _roomService.GetAvailableRoomsAsync(request, cancellationToken);
        return Ok(result);
    }
}