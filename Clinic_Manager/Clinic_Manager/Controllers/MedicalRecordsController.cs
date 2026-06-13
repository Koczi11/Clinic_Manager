using Clinic_Manager.DTOs;
using Clinic_Manager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Manager.Controllers;

[ApiController]
[Route("api")]
[Authorize(Roles = "Admin,Lekarz,Rejestratorka")]
public class MedicalRecordsController : ControllerBase
{
    private readonly IMedicalRecordService _medicalRecordService;

    public MedicalRecordsController(IMedicalRecordService medicalRecordService)
    {
        _medicalRecordService = medicalRecordService;
    }

    [HttpGet("patients/{patientId:int}/medicalrecords")]
    [ProducesResponseType(typeof(List<MedicalRecordDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<MedicalRecordDto>>> GetByPatientId(int patientId)
    {
        var records = await _medicalRecordService.GetByPatientIdAsync(patientId);
        return Ok(records);
    }

    [HttpPost("patients/{patientId:int}/medicalrecords")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(MedicalRecordDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MedicalRecordDto>> UploadRecord(int patientId, [FromForm] UploadRecordDto dto)
    {
        if (patientId != dto.PatientId)
        {
            return BadRequest("Identyfikator pacjenta w ścieżce nie zgadza się z identyfikatorem w przesyłanych danych.");
        }

        try
        {
            var createdRecord = await _medicalRecordService.AddRecordAsync(dto);
            return CreatedAtAction(
                nameof(GetByPatientId), 
                new { patientId = createdRecord.PatientId }, 
                createdRecord
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("medicalrecords/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _medicalRecordService.DeleteRecordAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
