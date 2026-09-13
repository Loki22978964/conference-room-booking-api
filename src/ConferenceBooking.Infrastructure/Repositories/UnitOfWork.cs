using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _appDbContext;
        private IRoomRepository? _roomRepository;

        public UnitOfWork(AppDbContext context)
        {
            _appDbContext = context;
        }

        public  IRoomRepository Rooms => _roomRepository ??= new RoomRepository(_appDbContext);

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _appDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
