using ConferenceBooking.Application.DTOs;
using ConferenceBooking.Application.Interfaces;
using ConferenceBooking.Application.Specifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Application.Services;

/// <summary>
/// Generates revenue reports aggregated by room over a given date range,
/// available as structured data or a downloadable CSV export.
/// </summary>
public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Builds a revenue report grouped by room for bookings starting within
    /// the given date range.
    /// </summary>
    /// <param name="startDate">The start of the reporting period (inclusive).</param>
    /// <param name="endDate">The end of the reporting period (inclusive).</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A report with per-room booking counts and revenue totals, sorted by revenue descending.</returns>
    /// <remarks>
    /// <paramref name="startDate"/> and <paramref name="endDate"/> are converted via
    /// <see cref="DateTime.ToUniversalTime"/>. If either has <see cref="DateTimeKind.Unspecified"/>,
    /// it is treated as local server time — callers should ensure the intended time zone
    /// semantics match this behavior, or pass values that are already UTC.
    /// </remarks>
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

    /// <summary>
    /// Builds the same revenue report as <see cref="GetRevenueReportAsync"/> and
    /// renders it as a UTF-8 encoded CSV file.
    /// </summary>
    /// <param name="startDate">The start of the reporting period (inclusive).</param>
    /// <param name="endDate">The end of the reporting period (inclusive).</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The CSV content as a UTF-8 byte array, with a header row and a trailing totals row.</returns>
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