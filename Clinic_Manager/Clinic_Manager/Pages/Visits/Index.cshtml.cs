using Clinic_Manager.DTOs;
using Clinic_Manager.Models;
using Clinic_Manager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Clinic_Manager.Pages.Visits;

[Authorize(Roles = "Admin,Lekarz,Rejestratorka")]
public class IndexModel : PageModel
{
    private readonly IVisitService _visitService;
    private readonly UserManager<IdentityUser> _userManager;

    public IndexModel(IVisitService visitService, UserManager<IdentityUser> userManager)
    {
        _visitService = visitService;
        _userManager = userManager;
    }

    public List<VisitDto> Visits { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public VisitStatus? Status { get; set; }

    public async Task OnGetAsync()
    {
        // Jeśli zalogowany to Lekarz i nie jest Adminem/Rejestratorką, widzi tylko swoje wizyty.
        if (User.IsInRole("Lekarz") && !User.IsInRole("Admin") && !User.IsInRole("Rejestratorka"))
        {
            var doctorId = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(doctorId))
            {
                var allDoctorVisits = await _visitService.GetByDoctorIdAsync(doctorId);
                Visits = Status.HasValue 
                    ? allDoctorVisits.Where(v => v.Status == Status.Value).ToList() 
                    : allDoctorVisits;
            }
        }
        else
        {
            // Rejestratorka i Admin widzą wszystkie
            Visits = await _visitService.GetAllAsync(Status);
        }
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int id, VisitStatus newStatus)
    {
        await _visitService.UpdateStatusAsync(id, newStatus);
        TempData["SuccessMessage"] = "Status wizyty został zaktualizowany.";
        return RedirectToPage(new { Status });
    }
}
