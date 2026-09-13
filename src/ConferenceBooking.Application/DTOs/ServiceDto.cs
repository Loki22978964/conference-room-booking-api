using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Application.DTOs;

public class ServiceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
