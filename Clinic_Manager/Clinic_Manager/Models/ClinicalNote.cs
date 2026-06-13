using System.ComponentModel.DataAnnotations;

namespace Clinic_Manager.Models;

public class ClinicalNote
{
    public int Id { get; set; }

    [Required]
    public int VisitId { get; set; }
    public Visit Visit { get; set; } = null!;

    [Required(ErrorMessage = "Treść notatki jest wymagana.")]
    public string Content { get; set; } = string.Empty;

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    [Required]
    public string AuthorName { get; set; } = string.Empty;
}
