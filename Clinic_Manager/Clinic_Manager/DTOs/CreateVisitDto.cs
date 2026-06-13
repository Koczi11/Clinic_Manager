using System.ComponentModel.DataAnnotations;

namespace Clinic_Manager.DTOs;

public class CreateVisitDto
{
    [Required(ErrorMessage = "Wybór pacjenta jest wymagany.")]
    [Display(Name = "Pacjent")]
    public int PatientId { get; set; }

    [Required(ErrorMessage = "Wybór lekarza jest wymagany.")]
    [Display(Name = "Lekarz")]
    public string DoctorId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Data i godzina wizyty są wymagane.")]
    [Display(Name = "Data i godzina wizyty")]
    public DateTime VisitDate { get; set; }

    [StringLength(500, ErrorMessage = "Opis wizyty nie może przekraczać 500 znaków.")]
    [Display(Name = "Cel wizyty / Opis")]
    public string? Description { get; set; }
}
