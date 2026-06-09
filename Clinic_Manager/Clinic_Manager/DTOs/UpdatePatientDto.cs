using System.ComponentModel.DataAnnotations;

namespace Clinic_Manager.DTOs;

public class UpdatePatientDto
{
    [Required(ErrorMessage = "Imię jest wymagane.")]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nazwisko jest wymagane.")]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Data urodzenia jest wymagana.")]
    [DataType(DataType.Date)]
    public DateOnly DateOfBirth { get; set; }

    [Phone(ErrorMessage = "Nieprawidłowy numer telefonu.")]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [EmailAddress(ErrorMessage = "Nieprawidłowy adres e-mail.")]
    [StringLength(256)]
    public string? Email { get; set; }

    [StringLength(250)]
    public string? Address { get; set; }
}
