using ConferenceBooking.Application.DTOs;
using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Domain.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateRoom([FromBody] CreateOrUpdateRoomRequest request, CancellationToken cancellationToken)
    {
        var roomId = await _roomService.CreateRoomAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetRoom), new { id = roomId }, new { Id = roomId });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoom(Guid id, CancellationToken cancellationToken)
    {
        var dto = await _roomService.GetRoomAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoom(Guid id, [FromBody] CreateOrUpdateRoomRequest request, CancellationToken cancellationToken)
    {
        var updated = await _roomService.UpdateRoomAsync(id, request, cancellationToken);
        return updated ? Ok(new { Message = "Room updated successfully" }) : NotFound("Room not found");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoom(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _roomService.DeleteRoomAsync(id, cancellationToken);
        return deleted ? Ok(new { Message = "Room deleted successfully" }) : NotFound();
    }
}