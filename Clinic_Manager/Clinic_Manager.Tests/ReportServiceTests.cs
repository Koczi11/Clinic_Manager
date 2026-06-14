using Clinic_Manager.Data;
using Clinic_Manager.DTOs;
using Clinic_Manager.Models;
using Clinic_Manager.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuestPDF.Infrastructure;

namespace Clinic_Manager.Tests;

public class ReportServiceTests
{
    static ReportServiceTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task SeedCompletedVisitAsync(ApplicationDbContext context)
    {
        var patient = new Patient { Pesel = "90010112345", FirstName = "Jan", LastName = "Kowalski", DateOfBirth = new DateOnly(1990, 1, 1) };
        var doctor = new IdentityUser { Id = "doc-1", UserName = "lekarz@clinic.com", Email = "lekarz@clinic.com" };
        var procedure = new Procedure { Name = "Konsultacja", Cost = 150m };

        context.Patients.Add(patient);
        context.Users.Add(doctor);
        context.Procedures.Add(procedure);
        await context.SaveChangesAsync();

        var visit = new Visit
        {
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            VisitDate = new DateTime(2026, 6, 10, 10, 0, 0),
            Status = VisitStatus.Completed
        };
        context.Visits.Add(visit);
        await context.SaveChangesAsync();

        context.ProceduresPerformed.Add(new ProcedurePerformed { VisitId = visit.Id, ProcedureId = procedure.Id });
        await context.SaveChangesAsync();
    }

    private static ReportService CreateService(ApplicationDbContext context) =>
        new(context, NullLogger<ReportService>.Instance);

    [Fact]
    public async Task GetCostReportDataAsync_SumsCompletedVisitCosts()
    {
        var context = CreateContext();
        await SeedCompletedVisitAsync(context);
        var service = CreateService(context);

        var data = await service.GetCostReportDataAsync(new CostReportFilter());

        Assert.Single(data.Rows);
        Assert.Equal(150m, data.GrandTotal);
        Assert.Equal("Kowalski Jan", data.Rows[0].PatientName);
    }

    [Fact]
    public async Task GetCostReportDataAsync_IgnoresNonCompletedVisits()
    {
        var context = CreateContext();
        context.Visits.Add(new Visit { PatientId = 1, DoctorId = "x", VisitDate = DateTime.Now, Status = VisitStatus.Scheduled });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var data = await service.GetCostReportDataAsync(new CostReportFilter());

        Assert.Empty(data.Rows);
        Assert.Equal(0m, data.GrandTotal);
    }

    [Fact]
    public async Task GenerateCostReportPdfAsync_ReturnsValidPdfBytes()
    {
        var context = CreateContext();
        await SeedCompletedVisitAsync(context);
        var service = CreateService(context);

        var pdf = await service.GenerateCostReportPdfAsync(new CostReportFilter());

        Assert.NotNull(pdf);
        Assert.True(pdf.Length > 0);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(pdf, 0, 4));
    }
}
