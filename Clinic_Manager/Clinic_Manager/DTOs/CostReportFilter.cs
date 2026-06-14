namespace Clinic_Manager.DTOs;

public class CostReportFilter
{
    public int? PatientId { get; set; }
    public string? DoctorId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
