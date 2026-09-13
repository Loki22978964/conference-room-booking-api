using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Domain.Entities;

public class RoomService
{
    public Guid RoomId { get; private set; }
    public Room Room { get; private set; } = null!;

    public Guid ServiceId { get; private set; }
    public Service Service { get; private set; } = null!;

    private RoomService() { }

    public RoomService(Guid roomId, Guid serviceId)
    {
        RoomId = roomId;
        ServiceId = serviceId;
    }
}