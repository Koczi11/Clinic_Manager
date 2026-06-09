using Clinic_Manager.DTOs;
using Clinic_Manager.Models;
using Riok.Mapperly.Abstractions;

namespace Clinic_Manager.Mappers;

[Mapper]
public partial class PatientMapper
{
    [MapperIgnoreSource(nameof(Patient.IsDeleted))]
    [MapperIgnoreSource(nameof(Patient.DeletedAt))]
    public partial PatientDto ToDto(Patient patient);
    public partial List<PatientDto> ToDtoList(IEnumerable<Patient> patients);

    [MapperIgnoreTarget(nameof(Patient.Id))]
    [MapperIgnoreTarget(nameof(Patient.IsDeleted))]
    [MapperIgnoreTarget(nameof(Patient.DeletedAt))]
    public partial Patient ToEntity(CreatePatientDto dto);

    [MapperIgnoreTarget(nameof(Patient.Id))]
    [MapperIgnoreTarget(nameof(Patient.Pesel))]
    [MapperIgnoreTarget(nameof(Patient.IsDeleted))]
    [MapperIgnoreTarget(nameof(Patient.DeletedAt))]
    public partial void UpdateEntity(UpdatePatientDto dto, Patient patient);
}
