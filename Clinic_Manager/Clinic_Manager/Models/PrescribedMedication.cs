using System.ComponentModel.DataAnnotations;

namespace Clinic_Manager.Models;

public class PrescribedMedication
{
    public int Id { get; set; }

    [Required]
    public int VisitId { get; set; }
    public Visit Visit { get; set; } = null!;

    [Required]
    public int MedicationId { get; set; }
    public Medication Medication { get; set; } = null!;

    [Required(ErrorMessage = "Dawkowanie jest wymagane.")]
    [StringLength(200)]
    public string Dosage { get; set; } = string.Empty;

    [Required]
    [Range(1, 100, ErrorMessage = "Ilość opakowań musi być z przedziału od 1 do 100.")]
    public int Quantity { get; set; } = 1;
}
