using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Domain.Entities;

public class Room : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public int Capacity { get; private set; }
    public decimal BaseHourlyRate { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Навігаційні властивості для EF Core
    public ICollection<RoomService> RoomServices { get; private set; } = new List<RoomService>();
    public ICollection<Booking> Bookings { get; private set; } = new List<Booking>();

    // Порожній конструктор для EF Core
    private Room() { }

    public Room(string name, int capacity, decimal baseHourlyRate)
    {
        UpdateDetails(name, capacity, baseHourlyRate);
        Id = Guid.NewGuid();
        IsActive = true;
    }

    public void UpdateDetails(string name, int capacity, decimal baseHourlyRate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Room name cannot be empty.", nameof(name));
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be greater than zero.", nameof(capacity));
        if (baseHourlyRate < 0)
            throw new ArgumentException("Base hourly rate cannot be negative.", nameof(baseHourlyRate));

        Name = name;
        Capacity = capacity;
        BaseHourlyRate = baseHourlyRate;
    }

    public void MarkAsDeleted()
    {
        IsActive = false;
    }
}