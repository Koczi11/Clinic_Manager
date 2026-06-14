using Clinic_Manager.Data;
using Clinic_Manager.DTOs;
using Clinic_Manager.Models;
using Clinic_Manager.Reports;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

namespace Clinic_Manager.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReportService> _logger;

    public ReportService(ApplicationDbContext context, ILogger<ReportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CostReportData> GetCostReportDataAsync(CostReportFilter filter)
    {
        var query = _context.Visits
            .Include(v => v.Patient)
            .Include(v => v.Doctor)
            .Include(v => v.ProceduresPerformed).ThenInclude(pp => pp.Procedure)
            .Include(v => v.PrescribedMedications).ThenInclude(pm => pm.Medication)
            .Where(v => v.Status == VisitStatus.Completed);

        if (filter.PatientId.HasValue)
        {
            query = query.Where(v => v.PatientId == filter.PatientId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.DoctorId))
        {
            query = query.Where(v => v.DoctorId == filter.DoctorId);
        }

        if (filter.DateFrom.HasValue)
        {
            query = query.Where(v => v.VisitDate >= filter.DateFrom.Value);
        }

        if (filter.DateTo.HasValue)
        {
            query = query.Where(v => v.VisitDate <= filter.DateTo.Value);
        }

        var visits = await query
            .OrderBy(v => v.VisitDate)
            .ToListAsync();

        var rows = visits.Select(v =>
        {
            var proceduresCost = v.ProceduresPerformed.Sum(pp => pp.Procedure.Cost);
            var medicationsCost = v.PrescribedMedications.Sum(pm => pm.Quantity * pm.Medication.UnitPrice);

            return new CostReportRow
            {
                VisitDate = v.VisitDate,
                PatientName = $"{v.Patient.LastName} {v.Patient.FirstName}",
                DoctorName = v.Doctor.UserName ?? v.Doctor.Email ?? v.DoctorId,
                ProceduresCost = proceduresCost,
                MedicationsCost = medicationsCost,
                TotalCost = proceduresCost + medicationsCost
            };
        }).ToList();

        return new CostReportData
        {
            GeneratedAt = DateTime.Now,
            FilterDescription = await BuildFilterDescriptionAsync(filter),
            Rows = rows,
            GrandTotal = rows.Sum(r => r.TotalCost)
        };
    }

    public async Task<byte[]> GenerateCostReportPdfAsync(CostReportFilter filter)
    {
        var data = await GetCostReportDataAsync(filter);

        _logger.LogInformation("Wygenerowano raport kosztów: {Count} pozycji, suma {Total:0.00} zł.",
            data.Rows.Count, data.GrandTotal);

        var document = new CostReportDocument(data);
        return document.GeneratePdf();
    }

    private async Task<string> BuildFilterDescriptionAsync(CostReportFilter filter)
    {
        var parts = new List<string>();

        if (filter.PatientId.HasValue)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == filter.PatientId.Value);
            parts.Add(patient is null ? $"Pacjent #{filter.PatientId}" : $"Pacjent: {patient.LastName} {patient.FirstName}");
        }
        else
        {
            parts.Add("Pacjent: wszyscy");
        }

        if (!string.IsNullOrWhiteSpace(filter.DoctorId))
        {
            var doctor = await _context.Users.FirstOrDefaultAsync(u => u.Id == filter.DoctorId);
            parts.Add(doctor is null ? "Lekarz: wybrany" : $"Lekarz: {doctor.UserName ?? doctor.Email}");
        }
        else
        {
            parts.Add("Lekarz: wszyscy");
        }

        var from = filter.DateFrom.HasValue ? filter.DateFrom.Value.ToString("yyyy-MM-dd") : "początek";
        var to = filter.DateTo.HasValue ? filter.DateTo.Value.ToString("yyyy-MM-dd") : "dziś";
        parts.Add($"Okres: {from} – {to}");

        return string.Join("  |  ", parts);
    }
}
