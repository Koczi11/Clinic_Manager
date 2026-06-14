using Clinic_Manager.Data;
using Clinic_Manager.DTOs;
using Clinic_Manager.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Manager.Services;

public class PatientService : IPatientService
{
    private readonly ApplicationDbContext _context;
    private readonly PatientMapper _mapper;
    private readonly ILogger<PatientService> _logger;

    public PatientService(
        ApplicationDbContext context,
        PatientMapper mapper,
        ILogger<PatientService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<PatientDto>> GetAllAsync()
    {
        // Filtr globalny soft delete pomija usuniętych pacjentów.
        var patients = await _context.Patients
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();

        return _mapper.ToDtoList(patients);
    }

    public async Task<PatientDto?> GetByIdAsync(int id)
    {
        // FirstOrDefault (nie Find) - aby zadziałał globalny filtr soft-delete.
        var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == id);
        return patient is null ? null : _mapper.ToDto(patient);
    }

    public async Task<PatientDto> CreateAsync(CreatePatientDto dto)
    {
        try
        {
            var patient = _mapper.ToEntity(dto);

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Utworzono pacjenta o Id {PatientId}.", patient.Id);
            return _mapper.ToDto(patient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas tworzenia pacjenta.");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(int id, UpdatePatientDto dto)
    {
        try
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == id);
            if (patient is null)
            {
                return false;
            }

            _mapper.UpdateEntity(dto, patient);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Zaktualizowano pacjenta o Id {PatientId}.", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas aktualizowania pacjenta o Id {PatientId}.", id);
            throw;
        }
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        try
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == id);
            if (patient is null)
            {
                return false;
            }

            patient.IsDeleted = true;
            patient.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Miękko usunięto pacjenta o Id {PatientId}.", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas miękkiego usuwania pacjenta o Id {PatientId}.", id);
            throw;
        }
    }

    public async Task<List<PatientDto>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetAllAsync();
        }

        var term = query.Trim();

        var patients = await _context.Patients
            .Where(p => p.LastName.Contains(term) || p.Pesel.Contains(term))
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();

        return _mapper.ToDtoList(patients);
    }
}
