using Clinic_Manager.DTOs;
using Clinic_Manager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Clinic_Manager.Pages.Visits;

[Authorize(Roles = "Admin,Lekarz,Rejestratorka")]
public class CreateModel : PageModel
{
    private readonly IVisitService _visitService;
    private readonly IPatientService _patientService;
    private readonly UserManager<IdentityUser> _userManager;

    public CreateModel(
        IVisitService visitService, 
        IPatientService patientService, 
        UserManager<IdentityUser> userManager)
    {
        _visitService = visitService;
        _patientService = patientService;
        _userManager = userManager;
    }

    [BindProperty]
    public CreateVisitDto Input { get; set; } = new();

    public List<SelectListItem> Patients { get; set; } = new();
    public List<SelectListItem> Doctors { get; set; } = new();

    public async Task OnGetAsync()
    {
        await PopulateSelectListsAsync();
        // Domyślna data ustawiona na jutro o 10:00
        Input.VisitDate = DateTime.Today.AddDays(1).AddHours(10);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync();
            return Page();
        }

        try
        {
            await _visitService.CreateAsync(Input);
            TempData["SuccessMessage"] = "Wizyta została pomyślnie zaplanowana.";
            return RedirectToPage("Index");
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateSelectListsAsync();
            return Page();
        }
    }

    private async Task PopulateSelectListsAsync()
    {
        var activePatients = await _patientService.GetAllAsync();
        Patients = activePatients
            .Select(p => new SelectListItem 
            { 
                Value = p.Id.ToString(), 
                Text = $"{p.LastName} {p.FirstName} (PESEL: {p.Pesel})" 
            })
            .OrderBy(p => p.Text)
            .ToList();

        var doctors = await _userManager.GetUsersInRoleAsync("Lekarz");
        Doctors = doctors
            .Select(d => new SelectListItem 
            { 
                Value = d.Id, 
                Text = d.Email ?? d.UserName 
            })
            .OrderBy(d => d.Text)
            .ToList();
    }
}
