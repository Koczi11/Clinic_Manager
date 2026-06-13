using Clinic_Manager.Data;
using Clinic_Manager.DTOs;
using Clinic_Manager.Mappers;
using Clinic_Manager.Models;
using Clinic_Manager.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;

namespace Clinic_Manager.Tests;

public class VisitServiceTests
{
    private class FakeHttpContextAccessor : IHttpContextAccessor
    {
        public FakeHttpContextAccessor(string? username = null)
        {
            if (username != null)
            {
                var claims = new List<Claim> { new(ClaimTypes.Name, username) };
                var identity = new ClaimsIdentity(claims, "TestAuth");
                var principal = new ClaimsPrincipal(identity);
                HttpContext = new DefaultHttpContext { User = principal };
            }
            else
            {
                HttpContext = new DefaultHttpContext();
            }
        }

        public HttpContext? HttpContext { get; set; }
    }

    private static (VisitService service, ApplicationDbContext context) CreateService(IHttpContextAccessor? httpContextAccessor = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        var accessor = httpContextAccessor ?? new FakeHttpContextAccessor("TestDoctor");
        var service = new VisitService(
            context,
            new VisitMapper(),
            new ClinicalMapper(),
            accessor,
            NullLogger<VisitService>.Instance);
        return (service, context);
    }

    private static async Task<(Patient patient, IdentityUser doctor)> SeedDataAsync(ApplicationDbContext context)
    {
        var patient = new Patient
        {
            Pesel = "99010112345",
            FirstName = "Adam",
            LastName = "Kowalski",
            DateOfBirth = new DateOnly(1999, 1, 1)
        };
        context.Patients.Add(patient);

        var doctor = new IdentityUser
        {
            Id = "doc-1",
            UserName = "lekarz@clinic.com",
            Email = "lekarz@clinic.com"
        };
        context.Users.Add(doctor);

        await context.SaveChangesAsync();
        return (patient, doctor);
    }

    [Fact]
    public async Task CreateAsync_AddsVisitAndReturnsDto_WhenValid()
    {
        var (service, context) = CreateService();
        var (patient, doctor) = await SeedDataAsync(context);

        var dto = new CreateVisitDto
        {
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            VisitDate = DateTime.Now.AddDays(1),
            Description = "Konsultacja ogólna"
        };

        var result = await service.CreateAsync(dto);

        Assert.True(result.Id > 0);
        Assert.Equal(VisitStatus.Scheduled, result.Status);
        Assert.Equal("Adam Kowalski", result.PatientName);
        Assert.Equal("lekarz@clinic.com", result.DoctorName);
    }

    [Fact]
    public async Task CreateAsync_ThrowsArgumentException_WhenDateInPast()
    {
        var (service, context) = CreateService();
        var (patient, doctor) = await SeedDataAsync(context);

        var dto = new CreateVisitDto
        {
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            VisitDate = DateTime.Now.AddDays(-1),
            Description = "Wizyta w przeszłości"
        };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));
        Assert.Contains("Nie można zaplanować wizyty w przeszłości", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_ThrowsArgumentException_WhenPatientDoesNotExist()
    {
        var (service, context) = CreateService();
        var (_, doctor) = await SeedDataAsync(context);

        var dto = new CreateVisitDto
        {
            PatientId = 999,
            DoctorId = doctor.Id,
            VisitDate = DateTime.Now.AddDays(1)
        };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));
        Assert.Contains("Podany pacjent nie istnieje", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_ThrowsArgumentException_WhenDoctorDoesNotExist()
    {
        var (service, context) = CreateService();
        var (patient, _) = await SeedDataAsync(context);

        var dto = new CreateVisitDto
        {
            PatientId = patient.Id,
            DoctorId = "non-existent-doc-id",
            VisitDate = DateTime.Now.AddDays(1)
        };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(dto));
        Assert.Contains("Podany lekarz nie istnieje w systemie", ex.Message);
    }

    [Fact]
    public async Task GetByDoctorIdAsync_FiltersVisitsForDoctor()
    {
        var (service, context) = CreateService();
        var (patient, doctor) = await SeedDataAsync(context);

        var otherDoctor = new IdentityUser { Id = "doc-2", UserName = "other@clinic.com" };
        context.Users.Add(otherDoctor);
        await context.SaveChangesAsync();

        // Dodanie wizyt dla doctor
        var v1 = new Visit { PatientId = patient.Id, DoctorId = doctor.Id, VisitDate = DateTime.Now.AddDays(1), Status = VisitStatus.Scheduled };
        // Dodanie wizyt dla otherDoctor
        var v2 = new Visit { PatientId = patient.Id, DoctorId = otherDoctor.Id, VisitDate = DateTime.Now.AddDays(2), Status = VisitStatus.Scheduled };
        
        context.Visits.AddRange(v1, v2);
        await context.SaveChangesAsync();

        var result = await service.GetByDoctorIdAsync(doctor.Id);

        Assert.Single(result);
        Assert.Equal(doctor.Id, result[0].DoctorId);
    }

    [Fact]
    public async Task UpdateStatusAsync_ChangesStatusCorrectly()
    {
        var (service, context) = CreateService();
        var (patient, doctor) = await SeedDataAsync(context);

        var visit = new Visit { PatientId = patient.Id, DoctorId = doctor.Id, VisitDate = DateTime.Now.AddDays(1), Status = VisitStatus.Scheduled };
        context.Visits.Add(visit);
        await context.SaveChangesAsync();

        var result = await service.UpdateStatusAsync(visit.Id, VisitStatus.InProgress);

        Assert.True(result);
        var updated = await context.Visits.FindAsync(visit.Id);
        Assert.Equal(VisitStatus.InProgress, updated!.Status);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsVisitWithDetailsAndRodoLog_WhenExists()
    {
        var accessor = new FakeHttpContextAccessor("dr_audytor");
        var (service, context) = CreateService(accessor);
        var (patient, doctor) = await SeedDataAsync(context);

        var visit = new Visit
        {
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            VisitDate = DateTime.Now.AddDays(1),
            Status = VisitStatus.Scheduled
        };
        context.Visits.Add(visit);

        var proc = new Procedure { Name = "Badanie krwi", Cost = 50.00m };
        context.Procedures.Add(proc);

        var med = new Medication { Name = "Aspirin", UnitPrice = 10.00m, Unit = "op." };
        context.Medications.Add(med);
        await context.SaveChangesAsync();

        // Dodanie wpisów klinicznych
        var note = new ClinicalNote { VisitId = visit.Id, Content = "Pacjent skarży się na ból głowy.", AuthorName = "dr_audytor" };
        var procPerf = new ProcedurePerformed { VisitId = visit.Id, ProcedureId = proc.Id };
        var prescMed = new PrescribedMedication { VisitId = visit.Id, MedicationId = med.Id, Dosage = "1x dziennie", Quantity = 2 };

        context.ClinicalNotes.Add(note);
        context.ProceduresPerformed.Add(procPerf);
        context.PrescribedMedications.Add(prescMed);
        await context.SaveChangesAsync();

        var result = await service.GetByIdAsync(visit.Id);

        Assert.NotNull(result);
        Assert.Equal(visit.Id, result!.Id);
        Assert.Equal(70.00m, result.TotalCost); // 50 (procedura) + 2 * 10 (leki) = 70
        Assert.Single(result.ClinicalNotes);
        Assert.Equal("Pacjent skarży się na ból głowy.", result.ClinicalNotes[0].Content);
        Assert.Single(result.ProceduresPerformed);
        Assert.Equal("Badanie krwi", result.ProceduresPerformed[0].ProcedureName);
        Assert.Single(result.PrescribedMedications);
        Assert.Equal("Aspirin", result.PrescribedMedications[0].MedicationName);
        Assert.Equal(2, result.PrescribedMedications[0].Quantity);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenVisitNotFound()
    {
        var (service, _) = CreateService();
        var result = await service.GetByIdAsync(999);
        Assert.Null(result);
    }

    [Fact]
    public async Task AddClinicalNoteAsync_AddsNote_WhenValid()
    {
        var (service, context) = CreateService();
        var (patient, doctor) = await SeedDataAsync(context);

        var visit = new Visit { PatientId = patient.Id, DoctorId = doctor.Id, VisitDate = DateTime.Now.AddDays(1) };
        context.Visits.Add(visit);
        await context.SaveChangesAsync();

        var dto = new CreateClinicalNoteDto { Content = "Nowa notatka kliniczna" };
        var result = await service.AddClinicalNoteAsync(visit.Id, dto, "dr_kowalski");

        Assert.True(result.Id > 0);
        Assert.Equal("Nowa notatka kliniczna", result.Content);
        Assert.Equal("dr_kowalski", result.AuthorName);

        var dbNote = await context.ClinicalNotes.FindAsync(result.Id);
        Assert.NotNull(dbNote);
        Assert.Equal(visit.Id, dbNote!.VisitId);
    }

    [Fact]
    public async Task AddClinicalNoteAsync_ThrowsArgumentException_WhenVisitDoesNotExist()
    {
        var (service, _) = CreateService();
        var dto = new CreateClinicalNoteDto { Content = "Test" };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.AddClinicalNoteAsync(999, dto, "dr"));
        Assert.Contains("Podana wizyta nie istnieje", ex.Message);
    }

    [Fact]
    public async Task AddProcedurePerformedAsync_AddsProcedure_WhenValid()
    {
        var (service, context) = CreateService();
        var (patient, doctor) = await SeedDataAsync(context);

        var visit = new Visit { PatientId = patient.Id, DoctorId = doctor.Id, VisitDate = DateTime.Now.AddDays(1) };
        context.Visits.Add(visit);

        var proc = new Procedure { Name = "ECG", Cost = 120.00m };
        context.Procedures.Add(proc);
        await context.SaveChangesAsync();

        var result = await service.AddProcedurePerformedAsync(visit.Id, proc.Id);

        Assert.True(result.Id > 0);
        Assert.Equal("ECG", result.ProcedureName);
        Assert.Equal(120.00m, result.Cost);

        var dbProcPerf = await context.ProceduresPerformed.Include(x => x.Procedure).FirstOrDefaultAsync(x => x.Id == result.Id);
        Assert.NotNull(dbProcPerf);
        Assert.Equal(visit.Id, dbProcPerf!.VisitId);
        Assert.Equal(proc.Id, dbProcPerf.ProcedureId);
    }

    [Fact]
    public async Task AddProcedurePerformedAsync_ThrowsArgumentException_WhenVisitDoesNotExist()
    {
        var (service, _) = CreateService();
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.AddProcedurePerformedAsync(999, 1));
        Assert.Contains("Podana wizyta nie istnieje", ex.Message);
    }

    [Fact]
    public async Task AddProcedurePerformedAsync_ThrowsArgumentException_WhenProcedureDoesNotExist()
    {
        var (service, context) = CreateService();
        var (patient, doctor) = await SeedDataAsync(context);

        var visit = new Visit { PatientId = patient.Id, DoctorId = doctor.Id, VisitDate = DateTime.Now.AddDays(1) };
        context.Visits.Add(visit);
        await context.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.AddProcedurePerformedAsync(visit.Id, 999));
        Assert.Contains("Podana procedura nie istnieje", ex.Message);
    }

    [Fact]
    public async Task AddPrescribedMedicationAsync_AddsMedication_WhenValid()
    {
        var (service, context) = CreateService();
        var (patient, doctor) = await SeedDataAsync(context);

        var visit = new Visit { PatientId = patient.Id, DoctorId = doctor.Id, VisitDate = DateTime.Now.AddDays(1) };
        context.Visits.Add(visit);

        var med = new Medication { Name = "Ibuprom", UnitPrice = 15.50m, Unit = "tabletka" };
        context.Medications.Add(med);
        await context.SaveChangesAsync();

        var dto = new CreatePrescribedMedicationDto { MedicationId = med.Id, Dosage = "2x1", Quantity = 3 };
        var result = await service.AddPrescribedMedicationAsync(visit.Id, dto);

        Assert.True(result.Id > 0);
        Assert.Equal("Ibuprom", result.MedicationName);
        Assert.Equal(15.50m, result.UnitPrice);
        Assert.Equal("tabletka", result.Unit);
        Assert.Equal("2x1", result.Dosage);
        Assert.Equal(3, result.Quantity);

        var dbPrescMed = await context.PrescribedMedications.Include(x => x.Medication).FirstOrDefaultAsync(x => x.Id == result.Id);
        Assert.NotNull(dbPrescMed);
        Assert.Equal(visit.Id, dbPrescMed!.VisitId);
        Assert.Equal(med.Id, dbPrescMed.MedicationId);
    }

    [Fact]
    public async Task AddPrescribedMedicationAsync_ThrowsArgumentException_WhenVisitDoesNotExist()
    {
        var (service, _) = CreateService();
        var dto = new CreatePrescribedMedicationDto { MedicationId = 1, Dosage = "1", Quantity = 1 };
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.AddPrescribedMedicationAsync(999, dto));
        Assert.Contains("Podana wizyta nie istnieje", ex.Message);
    }

    [Fact]
    public async Task AddPrescribedMedicationAsync_ThrowsArgumentException_WhenMedicationDoesNotExist()
    {
        var (service, context) = CreateService();
        var (patient, doctor) = await SeedDataAsync(context);

        var visit = new Visit { PatientId = patient.Id, DoctorId = doctor.Id, VisitDate = DateTime.Now.AddDays(1) };
        context.Visits.Add(visit);
        await context.SaveChangesAsync();

        var dto = new CreatePrescribedMedicationDto { MedicationId = 999, Dosage = "1", Quantity = 1 };
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.AddPrescribedMedicationAsync(visit.Id, dto));
        Assert.Contains("Podany lek nie istnieje", ex.Message);
    }
}

