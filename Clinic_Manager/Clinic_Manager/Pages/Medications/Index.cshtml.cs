using Clinic_Manager.DTOs;
using Clinic_Manager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Clinic_Manager.Pages.Medications;

[Authorize(Roles = "Admin,Lekarz,Rejestratorka")]
public class IndexModel : PageModel
{
    private readonly IMedicationService _medicationService;

    public IndexModel(IMedicationService medicationService)
    {
        _medicationService = medicationService;
    }

    public List<MedicationDto> Medications { get; set; } = new();

    public async Task OnGetAsync()
    {
        Medications = await _medicationService.GetAllAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        if (!User.IsInRole("Admin") && !User.IsInRole("Rejestratorka"))
        {
            return Forbid();
        }

        await _medicationService.DeleteAsync(id);
        return RedirectToPage();
    }
}
