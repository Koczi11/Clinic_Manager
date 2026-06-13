using Clinic_Manager.Data;
using Clinic_Manager.DTOs;
using Clinic_Manager.Mappers;
using Clinic_Manager.Models;
using Clinic_Manager.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text;

namespace Clinic_Manager.Tests;

public class MedicalRecordServiceTests
{
    private class MockWebHostEnvironment : IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = Path.Combine(AppContext.BaseDirectory, "wwwroot");
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public string ApplicationName { get; set; } = "Clinic_Manager.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = null!;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
        public string EnvironmentName { get; set; } = "Testing";
    }

    private static (MedicalRecordService service, ApplicationDbContext context, string uploadsPath) CreateService()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        var mockEnv = new MockWebHostEnvironment();
        var service = new MedicalRecordService(
            context,
            new MedicalRecordMapper(),
            mockEnv,
            NullLogger<MedicalRecordService>.Instance
        );

        var uploadsPath = Path.Combine(mockEnv.WebRootPath, "uploads");
        return (service, context, uploadsPath);
    }

    private static IFormFile CreateMockFormFile(string fileName, string content, long length = -1)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);
        var fileLength = length >= 0 ? length : bytes.Length;
        return new FormFile(stream, 0, fileLength, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = fileName.EndsWith(".pdf") ? "application/pdf" : "image/jpeg"
        };
    }

    private static async Task<Patient> SeedPatientAsync(ApplicationDbContext context)
    {
        var patient = new Patient
        {
            Pesel = "12345678901",
            FirstName = "Jan",
            LastName = "Kowalski",
            DateOfBirth = new DateOnly(1990, 1, 1),
            InsuranceNumber = "INS-12345"
        };
        context.Patients.Add(patient);
        await context.SaveChangesAsync();
        return patient;
    }

    [Fact]
    public async Task AddRecordAsync_SavesFileAndAddsToDatabase()
    {
        var (service, context, uploadsPath) = CreateService();
        var patient = await SeedPatientAsync(context);

        var file = CreateMockFormFile("skierowanie.pdf", "Dummy PDF content");
        var dto = new UploadRecordDto
        {
            PatientId = patient.Id,
            Title = "Skierowanie na RTG",
            File = file
        };

        var result = await service.AddRecordAsync(dto);

        Assert.True(result.Id > 0);
        Assert.Equal("Skierowanie na RTG", result.Title);
        Assert.StartsWith("/uploads/", result.DocumentScanUrl);

        // Sprawdź czy plik fizycznie powstał
        var physicalFileName = Path.GetFileName(result.DocumentScanUrl);
        var physicalFilePath = Path.Combine(uploadsPath, physicalFileName);
        Assert.True(File.Exists(physicalFilePath));

        // Czyszczenie po teście
        if (File.Exists(physicalFilePath))
        {
            File.Delete(physicalFilePath);
        }
    }

    [Fact]
    public async Task AddRecordAsync_ThrowsArgumentException_WhenInvalidExtension()
    {
        var (service, context, _) = CreateService();
        var patient = await SeedPatientAsync(context);

        var file = CreateMockFormFile("script.exe", "dangerous code");
        var dto = new UploadRecordDto
        {
            PatientId = patient.Id,
            Title = "Złośliwy program",
            File = file
        };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.AddRecordAsync(dto));
        Assert.Contains("Dopuszczalne formaty plików to", ex.Message);
    }

    [Fact]
    public async Task AddRecordAsync_ThrowsArgumentException_WhenFileSizeTooLarge()
    {
        var (service, context, _) = CreateService();
        var patient = await SeedPatientAsync(context);

        // Długość pliku zasymulowana jako 6 MB
        var file = CreateMockFormFile("skan.jpg", "content", length: 6 * 1024 * 1024);
        var dto = new UploadRecordDto
        {
            PatientId = patient.Id,
            Title = "Wielki skan",
            File = file
        };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.AddRecordAsync(dto));
        Assert.Contains("Rozmiar pliku przekracza maksymalny limit 5 MB", ex.Message);
    }

    [Fact]
    public async Task AddRecordAsync_ThrowsArgumentException_WhenPatientDoesNotExist()
    {
        var (service, _, _) = CreateService();
        var file = CreateMockFormFile("skan.png", "content");
        var dto = new UploadRecordDto
        {
            PatientId = 999,
            Title = "Dokument",
            File = file
        };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.AddRecordAsync(dto));
        Assert.Contains("Podany pacjent nie istnieje", ex.Message);
    }

    [Fact]
    public async Task GetByPatientIdAsync_ReturnsOnlyRecordsForSpecificPatient()
    {
        var (service, context, uploadsPath) = CreateService();
        var p1 = await SeedPatientAsync(context);
        var p2 = new Patient { Pesel = "98765432109", FirstName = "Anna", LastName = "Nowak", DateOfBirth = new DateOnly(1985, 5, 5) };
        context.Patients.Add(p2);
        await context.SaveChangesAsync();

        var f1 = CreateMockFormFile("doc1.pdf", "content1");
        var f2 = CreateMockFormFile("doc2.png", "content2");

        var r1 = await service.AddRecordAsync(new UploadRecordDto { PatientId = p1.Id, Title = "P1 Doc", File = f1 });
        var r2 = await service.AddRecordAsync(new UploadRecordDto { PatientId = p2.Id, Title = "P2 Doc", File = f2 });

        var recordsP1 = await service.GetByPatientIdAsync(p1.Id);
        Assert.Single(recordsP1);
        Assert.Equal("P1 Doc", recordsP1[0].Title);

        // Czyszczenie
        File.Delete(Path.Combine(uploadsPath, Path.GetFileName(r1.DocumentScanUrl)));
        File.Delete(Path.Combine(uploadsPath, Path.GetFileName(r2.DocumentScanUrl)));
    }

    [Fact]
    public async Task DeleteRecordAsync_RemovesFromDatabaseAndDeletesFile()
    {
        var (service, context, uploadsPath) = CreateService();
        var patient = await SeedPatientAsync(context);

        var file = CreateMockFormFile("wyniki.jpg", "jpg data");
        var recordDto = await service.AddRecordAsync(new UploadRecordDto
        {
            PatientId = patient.Id,
            Title = "Wyniki",
            File = file
        });

        var physicalFilePath = Path.Combine(uploadsPath, Path.GetFileName(recordDto.DocumentScanUrl));
        Assert.True(File.Exists(physicalFilePath));

        var deleteResult = await service.DeleteRecordAsync(recordDto.Id);
        Assert.True(deleteResult);

        // Sprawdź czy usunięto z bazy
        var dbRecord = await context.MedicalRecords.FindAsync(recordDto.Id);
        Assert.Null(dbRecord);

        // Sprawdź czy plik zniknął z dysku
        Assert.False(File.Exists(physicalFilePath));
    }
}
