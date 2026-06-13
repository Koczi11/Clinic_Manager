using Clinic_Manager.DTOs;
using Clinic_Manager.Models;
using Riok.Mapperly.Abstractions;

namespace Clinic_Manager.Mappers;

[Mapper]
public partial class VisitMapper
{
    [MapperIgnoreSource(nameof(Visit.Patient))]
    [MapperIgnoreSource(nameof(Visit.Doctor))]
    [MapperIgnoreTarget(nameof(VisitDto.PatientName))]
    [MapperIgnoreTarget(nameof(VisitDto.DoctorName))]
    public partial VisitDto ToBasicDto(Visit entity);

    public VisitDto ToDto(Visit entity)
    {
        var dto = ToBasicDto(entity);
        if (entity.Patient != null)
        {
            dto.PatientName = $"{entity.Patient.FirstName} {entity.Patient.LastName}";
        }
        if (entity.Doctor != null)
        {
            dto.DoctorName = entity.Doctor.Email ?? entity.Doctor.UserName ?? string.Empty;
        }
        return dto;
    }

    public List<VisitDto> ToDtoList(IEnumerable<Visit> entities)
    {
        return entities.Select(ToDto).ToList();
    }

    [MapperIgnoreTarget(nameof(Visit.Id))]
    [MapperIgnoreTarget(nameof(Visit.Patient))]
    [MapperIgnoreTarget(nameof(Visit.Doctor))]
    [MapperIgnoreTarget(nameof(Visit.Status))]
    public partial Visit ToEntity(CreateVisitDto dto);
}
