using Clinic_Manager.DTOs;

namespace Clinic_Manager.Services;

public interface IPatientService
{
   Task<List<PatientDto>> GetAllAsync();

    Task<PatientDto?> GetByIdAsync(int id);

    Task<PatientDto> CreateAsync(CreatePatientDto dto);

    Task<bool> UpdateAsync(int id, UpdatePatientDto dto);

    Task<bool> SoftDeleteAsync(int id);

    Task<List<PatientDto>> SearchAsync(string query);
}
