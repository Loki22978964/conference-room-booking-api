using ConferenceBooking.Application.DTOs;
using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Application.Specifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Application.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<RevenueReportDto> GetRevenueReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var startUtc = startDate.ToUniversalTime();
        var endUtc = endDate.ToUniversalTime();

        var spec = new BookingsByDateRangeSpec(startUtc, endUtc);
        var bookings = await _unitOfWork.Bookings.ListAsync(spec, cancellationToken);

        var reportRows = bookings
            .GroupBy(b => new { b.RoomId, b.Room.Name })
            .Select(g => new RevenueReportRowDto
            {
                RoomId = g.Key.RoomId,
                RoomName = g.Key.Name,
                TotalBookings = g.Count(),
                TotalRevenue = g.Sum(b => b.TotalPrice)
            })
            .OrderByDescending(r => r.TotalRevenue)
            .ToList();

        return new RevenueReportDto
        {
            PeriodStart = startUtc,
            PeriodEnd = endUtc,
            TotalOverallRevenue = reportRows.Sum(r => r.TotalRevenue),
            RoomDetails = reportRows
        };
    }

    public async Task<byte[]> GetRevenueReportCsvAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var report = await GetRevenueReportAsync(startDate, endDate, cancellationToken);
        var builder = new StringBuilder();

        builder.AppendLine("Room ID,Room Name,Total Bookings,Total Revenue (UAH)");

        foreach (var row in report.RoomDetails)
        {
            builder.AppendLine($"{row.RoomId},{row.RoomName},{row.TotalBookings},{row.TotalRevenue}");
        }

        builder.AppendLine($",,TOTAL:,{report.TotalOverallRevenue}");
        return Encoding.UTF8.GetBytes(builder.ToString());
    }
}