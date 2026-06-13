using System.ComponentModel.DataAnnotations;

namespace Clinic_Manager.DTOs;

public class CreateClinicalNoteDto
{
    [Required(ErrorMessage = "Treść notatki jest wymagana.")]
    [Display(Name = "Treść notatki")]
    public string Content { get; set; } = string.Empty;
}
