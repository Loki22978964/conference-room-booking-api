using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Application.DTOs;

public class RevenueReportRowDto
{
    public Guid RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public int TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; }
}