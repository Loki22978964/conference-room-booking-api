using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Application.DTOs;

public class RevenueReportDto
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal TotalOverallRevenue { get; set; }
    public List<RevenueReportRowDto> RoomDetails { get; set; } = new();
}