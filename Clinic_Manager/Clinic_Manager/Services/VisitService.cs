using Clinic_Manager.Data;
using Clinic_Manager.DTOs;
using Clinic_Manager.Mappers;
using Clinic_Manager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Clinic_Manager.Services;

public class VisitService : IVisitService
{
    private readonly ApplicationDbContext _context;
    private readonly VisitMapper _mapper;
    private readonly ILogger<VisitService> _logger;

    public VisitService(
        ApplicationDbContext context,
        VisitMapper mapper,
        ILogger<VisitService> logger)
    {
        _context = context;
        _mapper = mapper;
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
            .FirstOrDefaultAsync(v => v.Id == id);

        return visit is null ? null : _mapper.ToDto(visit);
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
}
