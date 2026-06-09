using Clinic_Manager.DTOs;
using Clinic_Manager.Models;
using Riok.Mapperly.Abstractions;

namespace Clinic_Manager.Mappers;

[Mapper]
public partial class MedicationMapper
{
    public partial MedicationDto ToDto(Medication medication);

    public partial List<MedicationDto> ToDtoList(IEnumerable<Medication> medications);

    [MapperIgnoreTarget(nameof(Medication.Id))]
    public partial Medication ToEntity(CreateMedicationDto dto);

    [MapperIgnoreTarget(nameof(Medication.Id))]
    public partial void UpdateEntity(UpdateMedicationDto dto, Medication medication);
}
