using Clinic_Manager.Data;
using Clinic_Manager.DTOs;
using Clinic_Manager.Mappers;
using Clinic_Manager.Models;
using Clinic_Manager.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Clinic_Manager.Tests;

public class PatientServiceTests
{
    // Tworzy serwis na świeżej bazie InMemory (izolowanej per test).
    private static (PatientService service, ApplicationDbContext context) CreateService()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        var service = new PatientService(context, new PatientMapper(), NullLogger<PatientService>.Instance);
        return (service, context);
    }

    private static CreatePatientDto SamplePatient(string pesel = "90010112345", string lastName = "Kowalski") => new()
    {
        Pesel = pesel,
        FirstName = "Jan",
        LastName = lastName,
        DateOfBirth = new DateOnly(1990, 1, 1),
        PhoneNumber = "601234567",
        Email = "jan@example.com",
        Address = "ul. Testowa 1"
    };

    [Fact]
    public async Task CreateAsync_AddsPatientAndReturnsDtoWithId()
    {
        var (service, _) = CreateService();

        var result = await service.CreateAsync(SamplePatient());

        Assert.True(result.Id > 0);
        Assert.Equal("Kowalski", result.LastName);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyNonDeletedPatients()
    {
        var (service, _) = CreateService();
        await service.CreateAsync(SamplePatient("90010112345", "Kowalski"));
        var second = await service.CreateAsync(SamplePatient("85052254321", "Nowak"));

        await service.SoftDeleteAsync(second.Id);
        var all = await service.GetAllAsync();

        Assert.Single(all);
        Assert.Equal("Kowalski", all[0].LastName);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsPatient_WhenExists()
    {
        var (service, _) = CreateService();
        var created = await service.CreateAsync(SamplePatient());

        var found = await service.GetByIdAsync(created.Id);

        Assert.NotNull(found);
        Assert.Equal(created.Id, found!.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var (service, _) = CreateService();

        var found = await service.GetByIdAsync(999);

        Assert.Null(found);
    }

    [Fact]
    public async Task UpdateAsync_ChangesData_AndKeepsPeselUnchanged()
    {
        var (service, _) = CreateService();
        var created = await service.CreateAsync(SamplePatient());

        var updated = await service.UpdateAsync(created.Id, new UpdatePatientDto
        {
            FirstName = "Janusz",
            LastName = "Kowalski-Nowak",
            DateOfBirth = new DateOnly(1990, 1, 1),
            PhoneNumber = "600000000"
        });

        Assert.True(updated);
        var after = await service.GetByIdAsync(created.Id);
        Assert.Equal("Janusz", after!.FirstName);
        Assert.Equal("Kowalski-Nowak", after.LastName);
        // PESEL nie podlega edycji.
        Assert.Equal("90010112345", after.Pesel);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenNotFound()
    {
        var (service, _) = CreateService();

        var updated = await service.UpdateAsync(999, new UpdatePatientDto
        {
            FirstName = "X",
            LastName = "Y",
            DateOfBirth = new DateOnly(2000, 1, 1)
        });

        Assert.False(updated);
    }

    [Fact]
    public async Task SoftDeleteAsync_MarksDeleted_AndHidesFromGetById()
    {
        var (service, context) = CreateService();
        var created = await service.CreateAsync(SamplePatient());

        var deleted = await service.SoftDeleteAsync(created.Id);

        Assert.True(deleted);
        // Niewidoczny przez serwis (filtr globalny).
        Assert.Null(await service.GetByIdAsync(created.Id));
        // Ale rekord nadal istnieje w bazie (soft delete - RODO).
        var raw = await context.Patients.IgnoreQueryFilters().FirstAsync(p => p.Id == created.Id);
        Assert.True(raw.IsDeleted);
        Assert.NotNull(raw.DeletedAt);
    }

    [Fact]
    public async Task SoftDeleteAsync_ReturnsFalse_WhenNotFound()
    {
        var (service, _) = CreateService();

        var deleted = await service.SoftDeleteAsync(999);

        Assert.False(deleted);
    }

    [Fact]
    public async Task SearchAsync_FindsByLastName()
    {
        var (service, _) = CreateService();
        await service.CreateAsync(SamplePatient("90010112345", "Kowalski"));
        await service.CreateAsync(SamplePatient("85052254321", "Nowak"));

        var result = await service.SearchAsync("Nowak");

        Assert.Single(result);
        Assert.Equal("Nowak", result[0].LastName);
    }

    [Fact]
    public async Task SearchAsync_FindsByPeselFragment()
    {
        var (service, _) = CreateService();
        await service.CreateAsync(SamplePatient("90010112345", "Kowalski"));
        await service.CreateAsync(SamplePatient("85052254321", "Nowak"));

        var result = await service.SearchAsync("8505");

        Assert.Single(result);
        Assert.Equal("Nowak", result[0].LastName);
    }

    [Fact]
    public async Task SearchAsync_EmptyQuery_ReturnsAll()
    {
        var (service, _) = CreateService();
        await service.CreateAsync(SamplePatient("90010112345", "Kowalski"));
        await service.CreateAsync(SamplePatient("85052254321", "Nowak"));

        var result = await service.SearchAsync("   ");

        Assert.Equal(2, result.Count);
    }
}
