using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Clinic_Manager.DTOs;

public class UploadRecordDto
{
    [Required(ErrorMessage = "Identyfikator pacjenta jest wymagany.")]
    public int PatientId { get; set; }

    [Required(ErrorMessage = "Tytuł dokumentu jest wymagany.")]
    [StringLength(200, ErrorMessage = "Tytuł dokumentu nie może przekraczać 200 znaków.")]
    [Display(Name = "Tytuł dokumentu")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Wybierz plik dokumentu.")]
    [Display(Name = "Plik skanu/dokumentu (PDF, JPG, PNG, max 5MB)")]
    public IFormFile File { get; set; } = null!;
}
