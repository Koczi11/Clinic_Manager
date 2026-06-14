using Clinic_Manager.Data;
using Clinic_Manager.Models;
using Clinic_Manager.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using QuestPDF.Infrastructure;

namespace Clinic_Manager.Tests;

public class UpcomingVisitsReportBackgroundServiceTests
{
    static UpcomingVisitsReportBackgroundServiceTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private static IConfiguration CreateEmptyConfiguration()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"SmtpSettings:Host", ""},
            {"SmtpSettings:RecipientEmail", ""}
        };
        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    private static string FindSolutionRoot()
    {
        var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (currentDir != null)
        {
            if (currentDir.GetFiles("Clinic_Manager.slnx").Any() || currentDir.GetFiles("*.sln").Any())
            {
                return currentDir.FullName;
            }
            currentDir = currentDir.Parent;
        }
        return Directory.GetCurrentDirectory();
    }

    [Fact]
    public async Task GenerateAndSendReportAsync_FiltersCorrectVisitsAndGeneratesPdf()
    {
        // Setup ServiceCollection to get a real IServiceScopeFactory
        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        
        var serviceProvider = services.BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        using (var scope = scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Seed data
            var patient = new Patient { Pesel = "99010112345", FirstName = "Adam", LastName = "Kowalski", DateOfBirth = new DateOnly(1999, 1, 1) };
            var doctor = new IdentityUser { Id = "doc-1", UserName = "lekarz@clinic.com", Email = "lekarz@clinic.com" };
            context.Patients.Add(patient);
            context.Users.Add(doctor);
            await context.SaveChangesAsync();

            var tomorrow = DateTime.Today.AddDays(1);

            // 1. Visit scheduled for tomorrow (Should be included)
            context.Visits.Add(new Visit
            {
                PatientId = patient.Id,
                DoctorId = doctor.Id,
                VisitDate = tomorrow.AddHours(10),
                Status = VisitStatus.Scheduled,
                Description = "Jutrzejsza wizyta"
            });

            // 2. Visit completed for tomorrow (Should be ignored)
            context.Visits.Add(new Visit
            {
                PatientId = patient.Id,
                DoctorId = doctor.Id,
                VisitDate = tomorrow.AddHours(11),
                Status = VisitStatus.Completed,
                Description = "Zakończona jutrzejsza"
            });

            // 3. Visit scheduled for today (Should be ignored)
            context.Visits.Add(new Visit
            {
                PatientId = patient.Id,
                DoctorId = doctor.Id,
                VisitDate = DateTime.Today.AddHours(14),
                Status = VisitStatus.Scheduled,
                Description = "Dzisiejsza wizyta"
            });

            // 4. Visit scheduled for day after tomorrow (Should be ignored)
            context.Visits.Add(new Visit
            {
                PatientId = patient.Id,
                DoctorId = doctor.Id,
                VisitDate = tomorrow.AddDays(1).AddHours(9),
                Status = VisitStatus.Scheduled,
                Description = "Pojutrzejsza wizyta"
            });

            await context.SaveChangesAsync();
        }

        var config = CreateEmptyConfiguration();
        var logger = NullLogger<UpcomingVisitsReportBackgroundService>.Instance;
        var service = new UpcomingVisitsReportBackgroundService(scopeFactory, config, logger);

        // Path to local output file
        var projectRootPath = Path.Combine(Directory.GetCurrentDirectory(), "raport-nadchodzace-wizyty.pdf");
        
        // Ensure file does not exist before test
        if (File.Exists(projectRootPath))
        {
            File.Delete(projectRootPath);
        }

        // Act
        await service.GenerateAndSendReportAsync(CancellationToken.None);

        // Assert
        Assert.True(File.Exists(projectRootPath), "Raport PDF powinien zostać zapisany na dysku.");
        var fileInfo = new FileInfo(projectRootPath);
        Assert.True(fileInfo.Length > 0, "Raport PDF nie powinien być pusty.");

        // Copy to workspace roots for project documentation
        var solutionRoot = FindSolutionRoot();
        var workspaceRoot = Path.GetFullPath(Path.Combine(solutionRoot, ".."));
        var projectRoot = Path.Combine(solutionRoot, "Clinic_Manager");

        var mainRootPath = Path.Combine(workspaceRoot, "raport-nadchodzace-wizyty.pdf");
        var projectRootPathCopy = Path.Combine(projectRoot, "raport-nadchodzace-wizyty.pdf");
        try
        {
            File.Copy(projectRootPath, mainRootPath, true);
            File.Copy(projectRootPath, projectRootPathCopy, true);
        }
        catch
        {
            // Ignore if copy fails due to path issues
        }
    }
}
