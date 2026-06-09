using Clinic_Manager.DTOs;

namespace Clinic_Manager.Services;

public interface IMedicationService
{
    Task<List<MedicationDto>> GetAllAsync();

    Task<MedicationDto?> GetByIdAsync(int id);

    Task<MedicationDto> CreateAsync(CreateMedicationDto dto);

    Task<bool> UpdateAsync(int id, UpdateMedicationDto dto);

    Task<bool> DeleteAsync(int id);
}
