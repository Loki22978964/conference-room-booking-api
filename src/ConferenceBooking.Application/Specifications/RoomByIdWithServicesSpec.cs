using Ardalis.Specification;
using ConferenceBooking.Domain.Entities;

namespace ConferenceBooking.Application.Specifications;

public sealed class RoomByIdWithServicesSpec : Specification<Room>
{
    public RoomByIdWithServicesSpec(Guid id)
    {
        Query.Where(r => r.Id == id)
            .Include(r => r.RoomServices)
            .ThenInclude(rs => rs.Service);
    }
}