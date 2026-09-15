using ConferenceBooking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenueReportJson([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, CancellationToken cancellationToken)
    {
        var report = await _reportService.GetRevenueReportAsync(startDate, endDate, cancellationToken);
        return Ok(report);
    }

    [HttpGet("revenue/export")]
    public async Task<IActionResult> ExportRevenueReportCsv([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, CancellationToken cancellationToken)
    {
        var csvBytes = await _reportService.GetRevenueReportCsvAsync(startDate, endDate, cancellationToken);
        var fileName = $"revenue_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv";

        return File(csvBytes, "text/csv", fileName);
    }
}