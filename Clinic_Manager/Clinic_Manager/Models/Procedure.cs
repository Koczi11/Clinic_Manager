using System.ComponentModel.DataAnnotations;

namespace Clinic_Manager.Models;

public class Procedure
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Nazwa procedury jest wymagana.")]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Koszt procedury jest wymagany.")]
    [Range(0, 1_000_000, ErrorMessage = "Koszt musi być większy lub równy zero.")]
    public decimal Cost { get; set; }
}
