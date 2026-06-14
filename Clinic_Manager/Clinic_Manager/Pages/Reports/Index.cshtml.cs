using System.Globalization;
using Clinic_Manager.DTOs;
using Clinic_Manager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Clinic_Manager.Pages.Reports;

[Authorize(Roles = "Admin,Rejestratorka")]
public class IndexModel : PageModel
{
    private readonly IReportService _reportService;
    private readonly IPatientService _patientService;
    private readonly UserManager<IdentityUser> _userManager;

    public IndexModel(
        IReportService reportService,
        IPatientService patientService,
        UserManager<IdentityUser> userManager)
    {
        _reportService = reportService;
        _patientService = patientService;
        _userManager = userManager;
    }

    [BindProperty]
    public string? SelectedMonth { get; set; }

    [BindProperty]
    public int? PatientId { get; set; }

    [BindProperty]
    public string? DoctorId { get; set; }

    public List<SelectListItem> Months { get; set; } = new();
    public List<SelectListItem> Patients { get; set; } = new();
    public List<SelectListItem> Doctors { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadOptionsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var filter = new CostReportFilter
        {
            PatientId = PatientId,
            DoctorId = string.IsNullOrWhiteSpace(DoctorId) ? null : DoctorId
        };

        if (!string.IsNullOrWhiteSpace(SelectedMonth) &&
            DateTime.TryParseExact(SelectedMonth, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var month))
        {
            filter.DateFrom = new DateTime(month.Year, month.Month, 1);
            filter.DateTo = filter.DateFrom.Value.AddMonths(1).AddDays(-1).Date.AddHours(23).AddMinutes(59).AddSeconds(59);
        }

        var pdf = await _reportService.GenerateCostReportPdfAsync(filter);
        var fileName = $"raport-kosztow-{DateTime.Now:yyyyMMdd-HHmm}.pdf";

        return File(pdf, "application/pdf", fileName);
    }

    private async Task LoadOptionsAsync()
    {
        var culture = new CultureInfo("pl-PL");
        var now = DateTime.Now;
        for (var i = 0; i < 12; i++)
        {
            var date = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
            Months.Add(new SelectListItem
            {
                Value = date.ToString("yyyy-MM"),
                Text = date.ToString("MMMM yyyy", culture)
            });
        }

        var patients = await _patientService.GetAllAsync();
        Patients = patients
            .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = $"{p.LastName} {p.FirstName}" })
            .ToList();

        var doctors = await _userManager.GetUsersInRoleAsync("Lekarz");
        Doctors = doctors
            .Select(d => new SelectListItem { Value = d.Id, Text = d.UserName ?? d.Email })
            .ToList();
    }
}
