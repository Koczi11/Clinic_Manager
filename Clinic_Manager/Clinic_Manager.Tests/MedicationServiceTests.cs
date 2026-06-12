using Clinic_Manager.Data;
using Clinic_Manager.DTOs;
using Clinic_Manager.Mappers;
using Clinic_Manager.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Clinic_Manager.Tests;

public class MedicationServiceTests
{
    private static (MedicationService service, ApplicationDbContext context) CreateService()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        var service = new MedicationService(context, new MedicationMapper(), NullLogger<MedicationService>.Instance);
        return (service, context);
    }

    private static CreateMedicationDto SampleMedication(string name = "Apap", decimal price = 8.99m) => new()
    {
        Name = name,
        Description = "Lek przeciwbólowy",
        UnitPrice = price,
        Unit = "tabletka"
    };

    [Fact]
    public async Task CreateAsync_AddsMedicationAndReturnsDtoWithId()
    {
        var (service, _) = CreateService();

        var result = await service.CreateAsync(SampleMedication());

        Assert.True(result.Id > 0);
        Assert.Equal("Apap", result.Name);
        Assert.Equal(8.99m, result.UnitPrice);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllMedicationsOrderedByName()
    {
        var (service, _) = CreateService();
        await service.CreateAsync(SampleMedication("Zyrtec"));
        await service.CreateAsync(SampleMedication("Apap"));

        var all = await service.GetAllAsync();

        Assert.Equal(2, all.Count);
        Assert.Equal("Apap", all[0].Name);
        Assert.Equal("Zyrtec", all[1].Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsMedication_WhenExists()
    {
        var (service, _) = CreateService();
        var created = await service.CreateAsync(SampleMedication());

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
    public async Task UpdateAsync_ChangesData()
    {
        var (service, _) = CreateService();
        var created = await service.CreateAsync(SampleMedication());

        var updated = await service.UpdateAsync(created.Id, new UpdateMedicationDto
        {
            Name = "Apap Extra",
            Description = "Nowy opis",
            UnitPrice = 11.50m,
            Unit = "kapsułka"
        });

        Assert.True(updated);
        var after = await service.GetByIdAsync(created.Id);
        Assert.Equal("Apap Extra", after!.Name);
        Assert.Equal(11.50m, after.UnitPrice);
        Assert.Equal("kapsułka", after.Unit);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenNotFound()
    {
        var (service, _) = CreateService();

        var updated = await service.UpdateAsync(999, new UpdateMedicationDto
        {
            Name = "X",
            UnitPrice = 1m,
            Unit = "szt."
        });

        Assert.False(updated);
    }

    [Fact]
    public async Task DeleteAsync_RemovesMedication()
    {
        var (service, context) = CreateService();
        var created = await service.CreateAsync(SampleMedication());

        var deleted = await service.DeleteAsync(created.Id);

        Assert.True(deleted);
        Assert.Null(await service.GetByIdAsync(created.Id));
        Assert.False(await context.Medications.AnyAsync());
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
    {
        var (service, _) = CreateService();

        var deleted = await service.DeleteAsync(999);

        Assert.False(deleted);
    }
}
