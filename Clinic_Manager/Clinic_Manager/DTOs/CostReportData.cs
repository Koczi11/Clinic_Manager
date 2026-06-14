namespace Clinic_Manager.DTOs;

public class CostReportRow
{
    public DateTime VisitDate { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public decimal ProceduresCost { get; set; }
    public decimal MedicationsCost { get; set; }
    public decimal TotalCost { get; set; }
}

public class CostReportData
{
    public DateTime GeneratedAt { get; set; }
    public string FilterDescription { get; set; } = string.Empty;
    public List<CostReportRow> Rows { get; set; } = new();
    public decimal GrandTotal { get; set; }
}
