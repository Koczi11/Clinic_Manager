using Clinic_Manager.DTOs;
using Clinic_Manager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Clinic_Manager.Pages.Patients;

[Authorize(Roles = "Admin,Lekarz,Rejestratorka")]
public class IndexModel : PageModel
{
    private readonly IPatientService _patientService;

    public IndexModel(IPatientService patientService)
    {
        _patientService = patientService;
    }

    public List<PatientDto> Patients { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Query { get; set; }

    public async Task OnGetAsync()
    {
        Patients = string.IsNullOrWhiteSpace(Query)
            ? await _patientService.GetAllAsync()
            : await _patientService.SearchAsync(Query);
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _patientService.SoftDeleteAsync(id);
        return RedirectToPage();
    }
}
