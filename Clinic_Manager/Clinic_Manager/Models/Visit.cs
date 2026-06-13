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
}
