using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Clinic_Manager.Models;

public class Visit
{
    public int Id { get; set; }

    [Required]
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    [Required]
    public string DoctorId { get; set; } = string.Empty;
    public IdentityUser Doctor { get; set; } = null!;

    [Required]
    public DateTime VisitDate { get; set; }

    [Required]
    public VisitStatus Status { get; set; } = VisitStatus.Scheduled;

    [StringLength(500)]
    public string? Description { get; set; }

    public ICollection<ProcedurePerformed> ProceduresPerformed { get; set; } = new List<ProcedurePerformed>();
    public ICollection<ClinicalNote> ClinicalNotes { get; set; } = new List<ClinicalNote>();
    public ICollection<PrescribedMedication> PrescribedMedications { get; set; } = new List<PrescribedMedication>();

    public decimal TotalCost =>
        ProceduresPerformed.Sum(p => p.Procedure.Cost) +
        PrescribedMedications.Sum(pm => pm.Quantity * pm.Medication.UnitPrice);
}
