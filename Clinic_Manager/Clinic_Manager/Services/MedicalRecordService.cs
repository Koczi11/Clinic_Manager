using Clinic_Manager.Data;
using Clinic_Manager.DTOs;
using Clinic_Manager.Mappers;
using Clinic_Manager.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Clinic_Manager.Services;

public class MedicalRecordService : IMedicalRecordService
{
    private readonly ApplicationDbContext _context;
    private readonly MedicalRecordMapper _mapper;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<MedicalRecordService> _logger;

    private static readonly string[] AllowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public MedicalRecordService(
        ApplicationDbContext context,
        MedicalRecordMapper mapper,
        IWebHostEnvironment env,
        ILogger<MedicalRecordService> logger)
    {
        _context = context;
        _mapper = mapper;
        _env = env;
        _logger = logger;
    }

    public async Task<List<MedicalRecordDto>> GetByPatientIdAsync(int patientId)
    {
        var records = await _context.MedicalRecords
            .Where(r => r.PatientId == patientId)
            .OrderByDescending(r => r.DateAdded)
            .ToListAsync();

        return _mapper.ToDtoList(records);
    }

    public async Task<MedicalRecordDto?> GetByIdAsync(int id)
    {
        var record = await _context.MedicalRecords.FirstOrDefaultAsync(r => r.Id == id);
        return record is null ? null : _mapper.ToDto(record);
    }

    public async Task<MedicalRecordDto> AddRecordAsync(UploadRecordDto dto)
    {
        if (dto.File == null || dto.File.Length == 0)
        {
            throw new ArgumentException("Plik nie może być pusty.");
        }

        if (dto.File.Length > MaxFileSizeBytes)
        {
            throw new ArgumentException("Rozmiar pliku przekracza maksymalny limit 5 MB.");
        }

        var extension = Path.GetExtension(dto.File.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException("Dopuszczalne formaty plików to: .pdf, .jpg, .jpeg, .png.");
        }

        // Sprawdź czy pacjent istnieje
        var patientExists = await _context.Patients.AnyAsync(p => p.Id == dto.PatientId);
        if (!patientExists)
        {
            throw new ArgumentException("Podany pacjent nie istnieje.");
        }

        // Zlokalizuj/utwórz katalog uploads
        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadsFolder = Path.Combine(webRoot, "uploads");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        // Generuj unikalną nazwę pliku
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        // Zapis fizyczny pliku na dysku
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await dto.File.CopyToAsync(stream);
        }

        // Utworzenie encji w bazie danych
        var record = _mapper.ToEntity(dto);
        record.DocumentScanUrl = $"/uploads/{uniqueFileName}";
        record.DateAdded = DateTime.UtcNow;

        _context.MedicalRecords.Add(record);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Dodano dokument kartoteki o ID {RecordId} dla pacjenta o ID {PatientId}. Ścieżka pliku: {Path}", 
            record.Id, record.PatientId, record.DocumentScanUrl);

        return _mapper.ToDto(record);
    }

    public async Task<bool> DeleteRecordAsync(int id)
    {
        var record = await _context.MedicalRecords.FirstOrDefaultAsync(r => r.Id == id);
        if (record == null)
        {
            return false;
        }

        // Usunięcie fizycznego pliku z dysku
        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var relativePath = record.DocumentScanUrl.TrimStart('/');
        var filePath = Path.Combine(webRoot, relativePath);

        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Usunięto fizyczny plik dokumentu: {Path}", filePath);
            }
            else
            {
                _logger.LogWarning("Fizyczny plik dokumentu nie istnieje na dysku: {Path}", filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Wystąpił błąd podczas usuwania fizycznego pliku: {Path}", filePath);
        }

        _context.MedicalRecords.Remove(record);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Usunięto rekord kartoteki o ID {RecordId} dla pacjenta o ID {PatientId}.", 
            record.Id, record.PatientId);

        return true;
    }
}
