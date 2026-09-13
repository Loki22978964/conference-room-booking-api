using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Domain.Entities;

public class Booking
{
    public Guid Id { get; private set; }
    public Guid RoomId { get; private set; }
    public Room Room { get; private set; } = null!;

    public DateTime StartTimeUtc { get; private set; }
    public DateTime EndTimeUtc { get; private set; }

    public decimal HourlyRateSnapshot { get; private set; }

    public decimal TotalPrice { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public ICollection<BookingService> BookedServices { get; private set; } = new List<BookingService>();

    private Booking() { }

    public Booking(
        Guid roomId,
        DateTime startTimeUtc,
        DateTime endTimeUtc,
        decimal hourlyRateSnapshot,
        IEnumerable<(Guid ServiceId, decimal Price)> selectedServices)
    {
        if (startTimeUtc >= endTimeUtc)
            throw new ArgumentException("Start time must be before end time.");

        if (startTimeUtc < DateTime.UtcNow.AddMinutes(-5))
            throw new ArgumentException("Cannot create a booking in the past.");

        Id = Guid.NewGuid();
        RoomId = roomId;
        StartTimeUtc = startTimeUtc;
        EndTimeUtc = endTimeUtc;
        HourlyRateSnapshot = hourlyRateSnapshot;
        CreatedAtUtc = DateTime.UtcNow;

        CalculateTotalPrice(selectedServices);
    }

    private void CalculateTotalPrice(IEnumerable<(Guid ServiceId, decimal Price)> selectedServices)
    {
        var durationHours = (decimal)(EndTimeUtc - StartTimeUtc).TotalHours;
        var roomCost = durationHours * HourlyRateSnapshot;

        decimal servicesCost = 0;
        foreach (var service in selectedServices)
        {
            BookedServices.Add(new BookingService(Id, service.ServiceId, service.Price));
            servicesCost += service.Price;
        }

        TotalPrice = roomCost + servicesCost;
    }
}
