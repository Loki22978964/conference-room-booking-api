using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Application.Interfaces
{
    public interface IUnitOfWork
    {
        IRoomRepository Rooms { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
