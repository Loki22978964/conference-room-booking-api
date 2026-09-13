using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Domain.Entities;
using ConferenceBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Infrastructure.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _appDbContext;

    public RoomRepository(AppDbContext context)
    {
        _appDbContext = context;
    }

    public async Task<Room?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _appDbContext.Rooms.FirstOrDefaultAsync(room =>  room.Id == id, cancellationToken);
    }

    public async Task<Room?> GetByIdWithServicesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _appDbContext.Rooms
            .Include(r => r.RoomServices)
            .ThenInclude(rs => rs.Service)
            .FirstOrDefaultAsync(room => room.Id == id, cancellationToken);
    }

    public void Add(Room room)
    {
        _appDbContext.Rooms.Add(room);
    }



    public void Remove(Room room)
    {
        _appDbContext.Rooms.Remove(room);
    }
}