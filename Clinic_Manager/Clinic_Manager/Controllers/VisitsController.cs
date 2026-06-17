using Clinic_Manager.DTOs;
using Clinic_Manager.Models;
using Clinic_Manager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Manager.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Lekarz,Rejestratorka")]
public class VisitsController : ControllerBase
{
    private readonly IVisitService _visitService;

    public VisitsController(IVisitService visitService)
    {
        _visitService = visitService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<VisitDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VisitDto>>> GetAll([FromQuery] VisitStatus? status)
    {
        var visits = await _visitService.GetAllAsync(status);
        return Ok(visits);
    }

    [HttpGet("today")]
    [ProducesResponseType(typeof(List<VisitDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VisitDto>>> GetToday()
    {
        var visits = await _visitService.GetTodayAsync();
        return Ok(visits);
    }

    [HttpGet("active")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<VisitDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VisitDto>>> GetActive()
    {
        var visits = await _visitService.GetActiveAsync();
        return Ok(visits);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(VisitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VisitDto>> GetById(int id)
    {
        var visit = await _visitService.GetByIdAsync(id);
        return visit is null ? NotFound() : Ok(visit);
    }

    [HttpGet("patient/{patientId:int}")]
    [ProducesResponseType(typeof(List<VisitDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VisitDto>>> GetByPatientId(int patientId)
    {
        var visits = await _visitService.GetByPatientIdAsync(patientId);
        return Ok(visits);
    }

    [HttpGet("doctor/{doctorId}")]
    [ProducesResponseType(typeof(List<VisitDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VisitDto>>> GetByDoctorId(string doctorId)
    {
        var visits = await _visitService.GetByDoctorIdAsync(doctorId);
        return Ok(visits);
    }

    [HttpPost]
    [ProducesResponseType(typeof(VisitDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VisitDto>> Create([FromBody] CreateVisitDto dto)
    {
        try
        {
            var created = await _visitService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateVisitStatusDto dto)
    {
        var result = await _visitService.UpdateStatusAsync(id, dto.Status);
        return result ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/notes")]
    [Authorize(Roles = "Admin,Lekarz")]
    [ProducesResponseType(typeof(ClinicalNoteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClinicalNoteDto>> AddNote(int id, [FromBody] CreateClinicalNoteDto dto)
    {
        try
        {
            var author = User.Identity?.Name ?? "Lekarz";
            var note = await _visitService.AddClinicalNoteAsync(id, dto, author);
            return CreatedAtAction(nameof(GetById), new { id = note.VisitId }, note);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:int}/procedures")]
    [Authorize(Roles = "Admin,Lekarz")]
    [ProducesResponseType(typeof(ProcedurePerformedDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProcedurePerformedDto>> AddProcedure(int id, [FromBody] AddProcedureRequest request)
    {
        try
        {
            var pp = await _visitService.AddProcedurePerformedAsync(id, request.ProcedureId);
            return CreatedAtAction(nameof(GetById), new { id = pp.VisitId }, pp);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:int}/medications")]
    [Authorize(Roles = "Admin,Lekarz")]
    [ProducesResponseType(typeof(PrescribedMedicationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PrescribedMedicationDto>> AddMedication(int id, [FromBody] CreatePrescribedMedicationDto dto)
    {
        try
        {
            var pm = await _visitService.AddPrescribedMedicationAsync(id, dto);
            return CreatedAtAction(nameof(GetById), new { id = pm.VisitId }, pm);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

public class AddProcedureRequest
{
    public int ProcedureId { get; set; }
}
