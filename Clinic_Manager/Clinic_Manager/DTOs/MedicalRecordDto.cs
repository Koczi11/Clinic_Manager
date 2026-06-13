namespace Clinic_Manager.DTOs;

public class MedicalRecordDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DocumentScanUrl { get; set; } = string.Empty;
    public DateTime DateAdded { get; set; }
}
