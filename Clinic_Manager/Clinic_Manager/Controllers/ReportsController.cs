using Clinic_Manager.DTOs;
using Clinic_Manager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Manager.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Rejestratorka")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("costs")]
    [Produces("application/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCostReport(
        [FromQuery] int? patientId,
        [FromQuery] string? doctorId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        var filter = new CostReportFilter
        {
            PatientId = patientId,
            DoctorId = doctorId,
            DateFrom = dateFrom,
            DateTo = dateTo
        };

        var pdf = await _reportService.GenerateCostReportPdfAsync(filter);
        var fileName = $"raport-kosztow-{DateTime.Now:yyyyMMdd-HHmm}.pdf";

        return File(pdf, "application/pdf", fileName);
    }
}
