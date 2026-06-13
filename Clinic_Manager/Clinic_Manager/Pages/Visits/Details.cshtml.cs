using Clinic_Manager.Data;
using Clinic_Manager.DTOs;
using Clinic_Manager.Models;
using Clinic_Manager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Manager.Pages.Visits;

[Authorize(Roles = "Admin,Lekarz,Rejestratorka")]
public class DetailsModel : PageModel
{
    private readonly IVisitService _visitService;
    private readonly IMedicationService _medicationService;
    private readonly ApplicationDbContext _context;

    public DetailsModel(
        IVisitService visitService, 
        IMedicationService medicationService, 
        ApplicationDbContext context)
    {
        _visitService = visitService;
        _medicationService = medicationService;
        _context = context;
    }

    public VisitDto Visit { get; set; } = new();

    public List<SelectListItem> AvailableProcedures { get; set; } = new();
    public List<SelectListItem> AvailableMedications { get; set; } = new();

    [BindProperty]
    public CreateClinicalNoteDto NewNote { get; set; } = new();

    [BindProperty]
    public int SelectedProcedureId { get; set; }

    [BindProperty]
    public CreatePrescribedMedicationDto NewMedication { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var visit = await _visitService.GetByIdAsync(id);
        if (visit is null)
        {
            return NotFound();
        }

        // Zabezpieczenie przed podglądem wizyty innego lekarza (lekarz widzi tylko swoje)
        if (User.IsInRole("Lekarz") && !User.IsInRole("Admin") && !User.IsInRole("Rejestratorka"))
        {
            if (visit.DoctorName != User.Identity?.Name)
            {
                return Forbid();
            }
        }

        Visit = visit;
        await PopulateDropdownsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAddNoteAsync(int id)
    {
        if (!User.IsInRole("Lekarz") && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        ModelState.Clear();
        if (!TryValidateModel(NewNote, nameof(NewNote)))
        {
            await ReloadPageDataAsync(id);
            return Page();
        }

        try
        {
            var author = User.Identity?.Name ?? "Lekarz";
            await _visitService.AddClinicalNoteAsync(id, NewNote, author);
            TempData["SuccessMessage"] = "Notatka kliniczna została dodana.";
            return RedirectToPage(new { id });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await ReloadPageDataAsync(id);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAddProcedureAsync(int id)
    {
        if (!User.IsInRole("Lekarz") && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        ModelState.Clear();
        if (SelectedProcedureId <= 0)
        {
            ModelState.AddModelError(string.Empty, "Wybierz poprawną procedurę.");
            await ReloadPageDataAsync(id);
            return Page();
        }

        try
        {
            await _visitService.AddProcedurePerformedAsync(id, SelectedProcedureId);
            TempData["SuccessMessage"] = "Procedura została przypisana do wizyty.";
            return RedirectToPage(new { id });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await ReloadPageDataAsync(id);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAddMedicationAsync(int id)
    {
        if (!User.IsInRole("Lekarz") && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        ModelState.Clear();
        if (!TryValidateModel(NewMedication, nameof(NewMedication)))
        {
            await ReloadPageDataAsync(id);
            return Page();
        }

        try
        {
            await _visitService.AddPrescribedMedicationAsync(id, NewMedication);
            TempData["SuccessMessage"] = "Lek został przepisany do wizyty.";
            return RedirectToPage(new { id });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await ReloadPageDataAsync(id);
            return Page();
        }
    }

    private async Task PopulateDropdownsAsync()
    {
        var procedures = await _context.Procedures.ToListAsync();
        AvailableProcedures = procedures
            .Select(p => new SelectListItem 
            { 
                Value = p.Id.ToString(), 
                Text = $"{p.Name} ({p.Cost.ToString("0.00")} zł)" 
            })
            .OrderBy(p => p.Text)
            .ToList();

        var medications = await _medicationService.GetAllAsync();
        AvailableMedications = medications
            .Select(m => new SelectListItem 
            { 
                Value = m.Id.ToString(), 
                Text = $"{m.Name} ({m.UnitPrice.ToString("0.00")} zł / {m.Unit})" 
            })
            .OrderBy(m => m.Text)
            .ToList();
    }

    private async Task ReloadPageDataAsync(int id)
    {
        var visit = await _visitService.GetByIdAsync(id);
        if (visit != null)
        {
            Visit = visit;
        }
        await PopulateDropdownsAsync();
    }
}
