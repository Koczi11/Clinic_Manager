namespace Clinic_Manager.DTOs;

public class PrescribedMedicationDto
{
    public int Id { get; set; }
    public int VisitId { get; set; }
    public int MedicationId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal TotalCost => Quantity * UnitPrice;
}
