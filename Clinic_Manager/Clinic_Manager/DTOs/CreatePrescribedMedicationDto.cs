using System.ComponentModel.DataAnnotations;

namespace Clinic_Manager.DTOs;

public class CreatePrescribedMedicationDto
{
    [Required(ErrorMessage = "Wybór leku jest wymagany.")]
    [Display(Name = "Lek")]
    public int MedicationId { get; set; }

    [Required(ErrorMessage = "Dawkowanie jest wymagane.")]
    [StringLength(200, ErrorMessage = "Opis dawkowania nie może przekraczać 200 znaków.")]
    [Display(Name = "Dawkowanie")]
    public string Dosage { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ilość jest wymagana.")]
    [Range(1, 100, ErrorMessage = "Ilość musi być z przedziału od 1 do 100.")]
    [Display(Name = "Ilość (opakowań/sztuk)")]
    public int Quantity { get; set; } = 1;
}
