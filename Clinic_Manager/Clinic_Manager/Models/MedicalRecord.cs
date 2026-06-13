using System.ComponentModel.DataAnnotations;

namespace Clinic_Manager.Models;

public class MedicalRecord
{
    public int Id { get; set; }

    [Required]
    public int PatientId { get; set; }

    public Patient Patient { get; set; } = null!;

    [Required(ErrorMessage = "Tytuł dokumentu jest wymagany.")]
    [StringLength(200, ErrorMessage = "Tytuł nie może przekraczać 200 znaków.")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string DocumentScanUrl { get; set; } = string.Empty;

    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
}
