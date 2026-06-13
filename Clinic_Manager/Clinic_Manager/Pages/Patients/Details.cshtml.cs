using System.ComponentModel.DataAnnotations;
using Clinic_Manager.DTOs;
using Clinic_Manager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Clinic_Manager.Pages.Patients;

[Authorize(Roles = "Admin,Lekarz,Rejestratorka")]
public class DetailsModel : PageModel
{
    private readonly IPatientService _patientService;
    private readonly IMedicalRecordService _medicalRecordService;

    public DetailsModel(IPatientService patientService, IMedicalRecordService medicalRecordService)
    {
        _patientService = patientService;
        _medicalRecordService = medicalRecordService;
    }

    public PatientDto Patient { get; set; } = new();
    public List<MedicalRecordDto> MedicalRecords { get; set; } = new();

    [BindProperty]
    public UploadInputModel UploadInput { get; set; } = new();

    public class UploadInputModel
    {
        [Required(ErrorMessage = "Tytuł dokumentu jest wymagany.")]
        [StringLength(200, ErrorMessage = "Tytuł nie może przekraczać 200 znaków.")]
        [Display(Name = "Tytuł dokumentu")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Wybierz plik do przesłania.")]
        [Display(Name = "Plik")]
        public IFormFile File { get; set; } = null!;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var patient = await _patientService.GetByIdAsync(id);
        if (patient is null)
        {
            return NotFound();
        }

        Patient = patient;
        MedicalRecords = await _medicalRecordService.GetByPatientIdAsync(id);
        return Page();
    }

    public async Task<IActionResult> OnPostUploadAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            var patient = await _patientService.GetByIdAsync(id);
            if (patient is null) return NotFound();
            Patient = patient;
            MedicalRecords = await _medicalRecordService.GetByPatientIdAsync(id);
            return Page();
        }

        try
        {
            var dto = new UploadRecordDto
            {
                PatientId = id,
                Title = UploadInput.Title,
                File = UploadInput.File
            };
            await _medicalRecordService.AddRecordAsync(dto);
            TempData["SuccessMessage"] = "Dokument został pomyślnie dodany do kartoteki.";
            return RedirectToPage(new { id });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var patient = await _patientService.GetByIdAsync(id);
            if (patient is null) return NotFound();
            Patient = patient;
            MedicalRecords = await _medicalRecordService.GetByPatientIdAsync(id);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteRecordAsync(int recordId, int id)
    {
        await _medicalRecordService.DeleteRecordAsync(recordId);
        TempData["SuccessMessage"] = "Dokument został usunięty z kartoteki.";
        return RedirectToPage(new { id });
    }
}
