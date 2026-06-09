using Clinic_Manager.DTOs;
using Clinic_Manager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Manager.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Lekarz,Rejestratorka")]
public class MedicationsController : ControllerBase
{
    private readonly IMedicationService _medicationService;

    public MedicationsController(IMedicationService medicationService)
    {
        _medicationService = medicationService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<MedicationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MedicationDto>>> GetAll()
    {
        return Ok(await _medicationService.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MedicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MedicationDto>> GetById(int id)
    {
        var medication = await _medicationService.GetByIdAsync(id);
        return medication is null ? NotFound() : Ok(medication);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Rejestratorka")]
    [ProducesResponseType(typeof(MedicationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MedicationDto>> Create([FromBody] CreateMedicationDto dto)
    {
        var created = await _medicationService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Rejestratorka")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMedicationDto dto)
    {
        var updated = await _medicationService.UpdateAsync(id, dto);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Rejestratorka")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _medicationService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
