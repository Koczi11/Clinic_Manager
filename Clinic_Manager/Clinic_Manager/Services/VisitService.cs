using Clinic_Manager.Data;
using Clinic_Manager.DTOs;
using Clinic_Manager.Mappers;
using Clinic_Manager.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Clinic_Manager.Services;

public class VisitService : IVisitService
{
    private readonly ApplicationDbContext _context;
    private readonly VisitMapper _mapper;
    private readonly ClinicalMapper _clinicalMapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<VisitService> _logger;

    public VisitService(
        ApplicationDbContext context,
        VisitMapper mapper,
        ClinicalMapper clinicalMapper,
        IHttpContextAccessor httpContextAccessor,
        ILogger<VisitService> logger)
    {
        _context = context;
        _mapper = mapper;
        _clinicalMapper = clinicalMapper;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<List<VisitDto>> GetAllAsync(VisitStatus? status = null)
    {
        var query = _context.Visits
            .Include(v => v.Patient)
            .Include(v => v.Doctor)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(v => v.Status == status.Value);
        }

        var visits = await query
            .OrderBy(v => v.VisitDate)
            .ToListAsync();

        return _mapper.ToDtoList(visits);
    }

    public async Task<List<VisitDto>> GetByPatientIdAsync(int patientId)
    {
        var visits = await _context.Visits
            .Include(v => v.Patient)
            .Include(v => v.Doctor)
            .Where(v => v.PatientId == patientId)
            .OrderBy(v => v.VisitDate)
            .ToListAsync();

        return _mapper.ToDtoList(visits);
    }

    public async Task<List<VisitDto>> GetByDoctorIdAsync(string doctorId)
    {
        var visits = await _context.Visits
            .Include(v => v.Patient)
            .Include(v => v.Doctor)
            .Where(v => v.DoctorId == doctorId)
            .OrderBy(v => v.VisitDate)
            .ToListAsync();

        return _mapper.ToDtoList(visits);
    }

    public async Task<VisitDto?> GetByIdAsync(int id)
    {
        var visit = await _context.Visits
            .Include(v => v.Patient)
            .Include(v => v.Doctor)
            .Include(v => v.ProceduresPerformed).ThenInclude(pp => pp.Procedure)
            .Include(v => v.ClinicalNotes)
            .Include(v => v.PrescribedMedications).ThenInclude(pm => pm.Medication)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (visit is null)
        {
            return null;
        }

        // RODO Audyt Logowanie
        var currentUser = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Niezalogowany";
        _logger.LogInformation("RODO Audyt: Użytkownik {User} wyświetlił dane medyczne wizyty o ID {VisitId} pacjenta o ID {PatientId} o godzinie {Time}.", 
            currentUser, visit.Id, visit.PatientId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        var dto = _mapper.ToDto(visit);
        dto.ProceduresPerformed = _clinicalMapper.ToDtoList(visit.ProceduresPerformed);
        dto.ClinicalNotes = _clinicalMapper.ToDtoList(visit.ClinicalNotes);
        dto.PrescribedMedications = _clinicalMapper.ToDtoList(visit.PrescribedMedications);
        dto.TotalCost = visit.TotalCost;

        return dto;
    }

    public async Task<VisitDto> CreateAsync(CreateVisitDto dto)
    {
        if (dto.VisitDate < DateTime.Now)
        {
            throw new ArgumentException("Nie można zaplanować wizyty w przeszłości.");
        }

        // Sprawdź czy pacjent istnieje
        var patientExists = await _context.Patients.AnyAsync(p => p.Id == dto.PatientId);
        if (!patientExists)
        {
            throw new ArgumentException("Podany pacjent nie istnieje.");
        }

        // Sprawdź czy lekarz istnieje w systemie Identity
        var doctorExists = await _context.Users.AnyAsync(u => u.Id == dto.DoctorId);
        if (!doctorExists)
        {
            throw new ArgumentException("Podany lekarz nie istnieje w systemie.");
        }

        var visit = _mapper.ToEntity(dto);
        visit.Status = VisitStatus.Scheduled;

        _context.Visits.Add(visit);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Utworzono wizytę o Id {VisitId} dla pacjenta {PatientId} i lekarza {DoctorId}.", 
            visit.Id, visit.PatientId, visit.DoctorId);

        // Pobierz ze świeżym Include, aby zmapować nazwy do DTO
        var createdVisit = await _context.Visits
            .Include(v => v.Patient)
            .Include(v => v.Doctor)
            .FirstAsync(v => v.Id == visit.Id);

        return _mapper.ToDto(createdVisit);
    }

    public async Task<bool> UpdateStatusAsync(int id, VisitStatus status)
    {
        var visit = await _context.Visits.FirstOrDefaultAsync(v => v.Id == id);
        if (visit is null)
        {
            return false;
        }

        var oldStatus = visit.Status;
        visit.Status = status;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Zmieniono status wizyty o Id {VisitId} z {OldStatus} na {NewStatus}.", 
            id, oldStatus, status);

        return true;
    }

    public async Task<ClinicalNoteDto> AddClinicalNoteAsync(int visitId, CreateClinicalNoteDto dto, string authorName)
    {
        var visitExists = await _context.Visits.AnyAsync(v => v.Id == visitId);
        if (!visitExists)
        {
            throw new ArgumentException("Podana wizyta nie istnieje.");
        }

        var note = _clinicalMapper.ToEntity(dto);
        note.VisitId = visitId;
        note.DateCreated = DateTime.UtcNow;
        note.AuthorName = authorName;

        _context.ClinicalNotes.Add(note);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Lekarz {Author} dodał notatkę kliniczną o ID {NoteId} do wizyty o ID {VisitId}.", 
            authorName, note.Id, visitId);

        return _clinicalMapper.ToDto(note);
    }

    public async Task<ProcedurePerformedDto> AddProcedurePerformedAsync(int visitId, int procedureId)
    {
        var visitExists = await _context.Visits.AnyAsync(v => v.Id == visitId);
        if (!visitExists)
        {
            throw new ArgumentException("Podana wizyta nie istnieje.");
        }

        var procedure = await _context.Procedures.FirstOrDefaultAsync(p => p.Id == procedureId);
        if (procedure == null)
        {
            throw new ArgumentException("Podana procedura nie istnieje.");
        }

        var pp = new ProcedurePerformed
        {
            VisitId = visitId,
            ProcedureId = procedureId
        };

        _context.ProceduresPerformed.Add(pp);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Dodano wykonaną procedurę '{ProcedureName}' (ID {ProcedureId}) do wizyty o ID {VisitId}.", 
            procedure.Name, procedureId, visitId);

        pp.Procedure = procedure;

        return _clinicalMapper.ToDto(pp);
    }

    public async Task<PrescribedMedicationDto> AddPrescribedMedicationAsync(int visitId, CreatePrescribedMedicationDto dto)
    {
        var visitExists = await _context.Visits.AnyAsync(v => v.Id == visitId);
        if (!visitExists)
        {
            throw new ArgumentException("Podana wizyta nie istnieje.");
        }

        var medication = await _context.Medications.FirstOrDefaultAsync(m => m.Id == dto.MedicationId);
        if (medication == null)
        {
            throw new ArgumentException("Podany lek nie istnieje.");
        }

        var pm = _clinicalMapper.ToEntity(dto);
        pm.VisitId = visitId;

        _context.PrescribedMedications.Add(pm);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Przepisano lek '{MedicationName}' (ID {MedicationId}) do wizyty o ID {VisitId}.", 
            medication.Name, dto.MedicationId, visitId);

        pm.Medication = medication;

        return _clinicalMapper.ToDto(pm);
    }
}
