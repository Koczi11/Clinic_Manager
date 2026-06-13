using Clinic_Manager.Data;
using Clinic_Manager.DTOs;
using Clinic_Manager.Mappers;
using Clinic_Manager.Models;
using Clinic_Manager.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Clinic_Manager.Tests;

public class VisitServiceTests
{
    private static (VisitService service, ApplicationDbContext context) CreateService()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        var service = new VisitService(context, new VisitMapper(), NullLogger<VisitService>.Instance);
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
}
