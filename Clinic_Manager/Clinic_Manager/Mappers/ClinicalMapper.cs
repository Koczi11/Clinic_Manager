using Clinic_Manager.DTOs;
using Clinic_Manager.Models;
using Riok.Mapperly.Abstractions;

namespace Clinic_Manager.Mappers;

[Mapper]
public partial class ClinicalMapper
{
    public partial ProcedureDto ToDto(Procedure entity);
    public partial List<ProcedureDto> ToDtoList(IEnumerable<Procedure> entities);

    [MapperIgnoreSource(nameof(ProcedurePerformed.Visit))]
    [MapperIgnoreSource(nameof(ProcedurePerformed.Procedure))]
    [MapperIgnoreTarget(nameof(ProcedurePerformedDto.ProcedureName))]
    [MapperIgnoreTarget(nameof(ProcedurePerformedDto.Cost))]
    public partial ProcedurePerformedDto ToBasicDto(ProcedurePerformed entity);

    public ProcedurePerformedDto ToDto(ProcedurePerformed entity)
    {
        var dto = ToBasicDto(entity);
        if (entity.Procedure != null)
        {
            dto.ProcedureName = entity.Procedure.Name;
            dto.Cost = entity.Procedure.Cost;
        }
        return dto;
    }

    public List<ProcedurePerformedDto> ToDtoList(IEnumerable<ProcedurePerformed> entities)
    {
        return entities.Select(ToDto).ToList();
    }

    [MapperIgnoreSource(nameof(ClinicalNote.Visit))]
    public partial ClinicalNoteDto ToDto(ClinicalNote entity);
    public partial List<ClinicalNoteDto> ToDtoList(IEnumerable<ClinicalNote> entities);
    
    [MapperIgnoreTarget(nameof(ClinicalNote.Id))]
    [MapperIgnoreTarget(nameof(ClinicalNote.Visit))]
    [MapperIgnoreTarget(nameof(ClinicalNote.DateCreated))]
    [MapperIgnoreTarget(nameof(ClinicalNote.AuthorName))]
    public partial ClinicalNote ToEntity(CreateClinicalNoteDto dto);

    [MapperIgnoreSource(nameof(PrescribedMedication.Visit))]
    [MapperIgnoreSource(nameof(PrescribedMedication.Medication))]
    [MapperIgnoreTarget(nameof(PrescribedMedicationDto.MedicationName))]
    [MapperIgnoreTarget(nameof(PrescribedMedicationDto.UnitPrice))]
    [MapperIgnoreTarget(nameof(PrescribedMedicationDto.Unit))]
    public partial PrescribedMedicationDto ToBasicDto(PrescribedMedication entity);

    public PrescribedMedicationDto ToDto(PrescribedMedication entity)
    {
        var dto = ToBasicDto(entity);
        if (entity.Medication != null)
        {
            dto.MedicationName = entity.Medication.Name;
            dto.UnitPrice = entity.Medication.UnitPrice;
            dto.Unit = entity.Medication.Unit;
        }
        return dto;
    }

    public List<PrescribedMedicationDto> ToDtoList(IEnumerable<PrescribedMedication> entities)
    {
        return entities.Select(ToDto).ToList();
    }

    [MapperIgnoreTarget(nameof(PrescribedMedication.Id))]
    [MapperIgnoreTarget(nameof(PrescribedMedication.Visit))]
    [MapperIgnoreTarget(nameof(PrescribedMedication.Medication))]
    public partial PrescribedMedication ToEntity(CreatePrescribedMedicationDto dto);
}
