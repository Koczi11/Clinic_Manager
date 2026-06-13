using Clinic_Manager.DTOs;
using Clinic_Manager.Models;

namespace Clinic_Manager.Services;

public interface IVisitService
{
    Task<List<VisitDto>> GetAllAsync(VisitStatus? status = null);
    Task<List<VisitDto>> GetByPatientIdAsync(int patientId);
    Task<List<VisitDto>> GetByDoctorIdAsync(string doctorId);
    Task<VisitDto?> GetByIdAsync(int id);
    Task<VisitDto> CreateAsync(CreateVisitDto dto);
    Task<bool> UpdateStatusAsync(int id, VisitStatus status);
}
