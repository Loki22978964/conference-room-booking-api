using ConferenceBooking.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceBooking.Application.Interfaces;

public interface IReportService
{
    Task<RevenueReportDto> GetRevenueReportAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<byte[]> GetRevenueReportCsvAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}