using Clinic_Manager.DTOs;
using Clinic_Manager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Clinic_Manager.Pages.Medications;

[Authorize(Roles = "Admin,Rejestratorka")]
public class EditModel : PageModel
{
    private readonly IMedicationService _medicationService;

    public EditModel(IMedicationService medicationService)
    {
        _medicationService = medicationService;
    }

    [BindProperty]
    public UpdateMedicationDto Input { get; set; } = new();

    [BindProperty]
    public int Id { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var medication = await _medicationService.GetByIdAsync(id);
        if (medication is null)
        {
            return NotFound();
        }

        Id = medication.Id;
        Input = new UpdateMedicationDto
        {
            Name = medication.Name,
            Description = medication.Description,
            UnitPrice = medication.UnitPrice,
            Unit = medication.Unit
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var updated = await _medicationService.UpdateAsync(Id, Input);
        if (!updated)
        {
            return NotFound();
        }

        return RedirectToPage("Index");
    }
}
