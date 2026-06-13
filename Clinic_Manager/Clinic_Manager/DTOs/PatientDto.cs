namespace Clinic_Manager.DTOs;

public class PatientDto
{
    public int Id { get; set; }
    public string Pesel { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? InsuranceNumber { get; set; }
}
