using Ardalis.Specification;
using ConferenceBooking.Domain.Entities;

namespace ConferenceBooking.Application.Specifications;

public class AvailableRoomsSpec : Specification<Room>
{
    public AvailableRoomsSpec(DateTime startUtc, DateTime endUtc, int capacity)
    {
        Query
            .Include(r => r.RoomServices)
            .ThenInclude(rs => rs.Service)
            .Where(r => r.Capacity >= capacity)
            .Where(r => !r.Bookings.Any(b => b.StartTimeUtc < endUtc && b.EndTimeUtc > startUtc));
    }
}
