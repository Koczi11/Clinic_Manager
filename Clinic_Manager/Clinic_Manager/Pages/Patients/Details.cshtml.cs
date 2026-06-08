using Clinic_Manager.DTOs;
using Clinic_Manager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Clinic_Manager.Pages.Patients;

[Authorize(Roles = "Admin,Lekarz,Rejestratorka")]
public class DetailsModel : PageModel
{
    private readonly IPatientService _patientService;

    public DetailsModel(IPatientService patientService)
    {
        _patientService = patientService;
    }

    public PatientDto Patient { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var patient = await _patientService.GetByIdAsync(id);
        if (patient is null)
        {
            return NotFound();
        }

        Patient = patient;
        return Page();
    }
}
