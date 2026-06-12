using Clinic_Manager.DTOs;
using Clinic_Manager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Clinic_Manager.Pages.Medications;

[Authorize(Roles = "Admin,Rejestratorka")]
public class CreateModel : PageModel
{
    private readonly IMedicationService _medicationService;

    public CreateModel(IMedicationService medicationService)
    {
        _medicationService = medicationService;
    }

    [BindProperty]
    public CreateMedicationDto Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _medicationService.CreateAsync(Input);
        return RedirectToPage("Index");
    }
}
