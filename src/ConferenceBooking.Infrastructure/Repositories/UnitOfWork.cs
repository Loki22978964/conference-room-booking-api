using ConferenceBooking.Application.Exceptions;
using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _appDbContext;
    private IRoomRepository? _roomRepository;
    private IBookingRepository? _bookingRepository;

    public UnitOfWork(AppDbContext context)
    {
        _appDbContext = context;
    }

    public IRoomRepository Rooms => _roomRepository ??= new RoomRepository(_appDbContext);

    public IBookingRepository Bookings => _bookingRepository ??= new BookingRepository(_appDbContext);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _appDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("no_overlapping_bookings") == true)
        {
            throw new OverlappingBookingException();
        }
    }
}