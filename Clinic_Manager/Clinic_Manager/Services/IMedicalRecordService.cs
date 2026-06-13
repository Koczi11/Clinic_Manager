using Clinic_Manager.DTOs;

namespace Clinic_Manager.Services;

public interface IMedicalRecordService
{
    Task<List<MedicalRecordDto>> GetByPatientIdAsync(int patientId);
    Task<MedicalRecordDto?> GetByIdAsync(int id);
    Task<MedicalRecordDto> AddRecordAsync(UploadRecordDto dto);
    Task<bool> DeleteRecordAsync(int id);
}
