using Ardalis.Specification;
using ConferenceBooking.Domain.Entities;

namespace ConferenceBooking.Application.Specifications;

public sealed class RoomByIdSpec : Specification<Room>
{
    public RoomByIdSpec(Guid id)
    {
        Query.Where(r => r.Id == id);
    }
}