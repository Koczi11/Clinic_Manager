using Clinic_Manager.Models;

namespace Clinic_Manager.DTOs;

public class VisitDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string DoctorId { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public DateTime VisitDate { get; set; }
    public VisitStatus Status { get; set; }
    public string? Description { get; set; }

    public List<ProcedurePerformedDto> ProceduresPerformed { get; set; } = new();
    public List<ClinicalNoteDto> ClinicalNotes { get; set; } = new();
    public List<PrescribedMedicationDto> PrescribedMedications { get; set; } = new();
    public decimal TotalCost { get; set; }
}
