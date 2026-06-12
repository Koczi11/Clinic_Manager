using Clinic_Manager.Data;
using Clinic_Manager.DTOs;
using Clinic_Manager.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Manager.Services;

public class MedicationService : IMedicationService
{
    private readonly ApplicationDbContext _context;
    private readonly MedicationMapper _mapper;
    private readonly ILogger<MedicationService> _logger;

    public MedicationService(
        ApplicationDbContext context,
        MedicationMapper mapper,
        ILogger<MedicationService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<MedicationDto>> GetAllAsync()
    {
        var medications = await _context.Medications
            .OrderBy(m => m.Name)
            .ToListAsync();

        return _mapper.ToDtoList(medications);
    }

    public async Task<MedicationDto?> GetByIdAsync(int id)
    {
        var medication = await _context.Medications.FindAsync(id);
        return medication is null ? null : _mapper.ToDto(medication);
    }

    public async Task<MedicationDto> CreateAsync(CreateMedicationDto dto)
    {
        var medication = _mapper.ToEntity(dto);

        _context.Medications.Add(medication);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Utworzono lek o Id {MedicationId}.", medication.Id);
        return _mapper.ToDto(medication);
    }

    public async Task<bool> UpdateAsync(int id, UpdateMedicationDto dto)
    {
        var medication = await _context.Medications.FindAsync(id);
        if (medication is null)
        {
            return false;
        }

        _mapper.UpdateEntity(dto, medication);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Zaktualizowano lek o Id {MedicationId}.", id);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var medication = await _context.Medications.FindAsync(id);
        if (medication is null)
        {
            return false;
        }

        _context.Medications.Remove(medication);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Usunięto lek o Id {MedicationId}.", id);
        return true;
    }
}
