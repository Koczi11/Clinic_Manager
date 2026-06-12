using System.ComponentModel.DataAnnotations;

namespace Clinic_Manager.DTOs;

public class UpdateMedicationDto
{
    [Required(ErrorMessage = "Nazwa leku jest wymagana.")]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Cena jednostkowa jest wymagana.")]
    [Range(0.01, 1_000_000, ErrorMessage = "Cena musi być większa od zera.")]
    public decimal UnitPrice { get; set; }

    [Required(ErrorMessage = "Jednostka miary jest wymagana.")]
    [StringLength(50)]
    public string Unit { get; set; } = string.Empty;
}
