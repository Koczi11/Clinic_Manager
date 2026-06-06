using System.ComponentModel.DataAnnotations;

namespace Clinic_Manager.Models;

public class Patient
{
    public int Id { get; set; }

    [Required(ErrorMessage = "PESEL jest wymagany.")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "PESEL musi składać się z dokładnie 11 cyfr.")]
    public string Pesel { get; set; } = string.Empty;

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

    // Soft delete
    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }
}
