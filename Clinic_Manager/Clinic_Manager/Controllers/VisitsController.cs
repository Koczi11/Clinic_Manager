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
}
