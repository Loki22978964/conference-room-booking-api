using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Domain.Entities;

public class Service : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }

    private Service() { }

    public Service(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Service name cannot be empty.", nameof(name));
        if (price < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(price));

        Id = Guid.NewGuid();
        Name = name;
        Price = price;
    }
}