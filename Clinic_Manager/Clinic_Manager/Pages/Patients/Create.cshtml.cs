using Clinic_Manager.DTOs;
using Clinic_Manager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Clinic_Manager.Pages.Patients;

[Authorize(Roles = "Admin,Lekarz,Rejestratorka")]
public class CreateModel : PageModel
{
    private readonly IPatientService _patientService;

    public CreateModel(IPatientService patientService)
    {
        _patientService = patientService;
    }

    [BindProperty]
    public CreatePatientDto Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _patientService.CreateAsync(Input);
        return RedirectToPage("Index");
    }
}
