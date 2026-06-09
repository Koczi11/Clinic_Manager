using Clinic_Manager.DTOs;
using Clinic_Manager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Clinic_Manager.Pages.Patients;

[Authorize(Roles = "Admin,Lekarz,Rejestratorka")]
public class EditModel : PageModel
{
    private readonly IPatientService _patientService;

    public EditModel(IPatientService patientService)
    {
        _patientService = patientService;
    }

    [BindProperty]
    public UpdatePatientDto Input { get; set; } = new();

    [BindProperty]
    public int Id { get; set; }

    // PESEL pokazujemy tylko do odczytu (nieedytowalny).
    public string Pesel { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var patient = await _patientService.GetByIdAsync(id);
        if (patient is null)
        {
            return NotFound();
        }

        Id = patient.Id;
        Pesel = patient.Pesel;
        Input = new UpdatePatientDto
        {
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            DateOfBirth = patient.DateOfBirth,
            PhoneNumber = patient.PhoneNumber,
            Email = patient.Email,
            Address = patient.Address
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var updated = await _patientService.UpdateAsync(Id, Input);
        if (!updated)
        {
            return NotFound();
        }

        return RedirectToPage("Index");
    }
}
