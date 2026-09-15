using ConferenceBooking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceBooking.Api.Controllers;

/// <summary>
/// Provides revenue reports aggregated by room over a given date range.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Returns a revenue report as JSON, grouped by room, for the given date range.
    /// </summary>
    /// <param name="startDate">The start of the reporting period (inclusive).</param>
    /// <param name="endDate">The end of the reporting period (inclusive).</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A revenue report with per-room totals and the overall total revenue.</returns>
    /// <response code="200">The report was generated successfully.</response>
    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenueReportJson([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, CancellationToken cancellationToken)
    {
        var report = await _reportService.GetRevenueReportAsync(startDate, endDate, cancellationToken);
        return Ok(report);
    }

    /// <summary>
    /// Exports the revenue report for the given date range as a downloadable CSV file.
    /// </summary>
    /// <param name="startDate">The start of the reporting period (inclusive).</param>
    /// <param name="endDate">The end of the reporting period (inclusive).</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A CSV file named "revenue_{startDate}_{endDate}.csv".</returns>
    /// <response code="200">The CSV file was generated successfully.</response>
    [HttpGet("revenue/export")]
    public async Task<IActionResult> ExportRevenueReportCsv([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, CancellationToken cancellationToken)
    {
        var csvBytes = await _reportService.GetRevenueReportCsvAsync(startDate, endDate, cancellationToken);
        var fileName = $"revenue_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv";

        return File(csvBytes, "text/csv", fileName);
    }
}