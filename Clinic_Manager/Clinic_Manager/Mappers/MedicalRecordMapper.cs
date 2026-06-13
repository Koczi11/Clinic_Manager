using Clinic_Manager.DTOs;
using Clinic_Manager.Models;
using Riok.Mapperly.Abstractions;

namespace Clinic_Manager.Mappers;

[Mapper]
public partial class MedicalRecordMapper
{
    [MapperIgnoreSource(nameof(MedicalRecord.Patient))]
    public partial MedicalRecordDto ToDto(MedicalRecord entity);

    public partial List<MedicalRecordDto> ToDtoList(IEnumerable<MedicalRecord> entities);

    [MapperIgnoreTarget(nameof(MedicalRecord.Id))]
    [MapperIgnoreTarget(nameof(MedicalRecord.Patient))]
    [MapperIgnoreTarget(nameof(MedicalRecord.DocumentScanUrl))]
    [MapperIgnoreTarget(nameof(MedicalRecord.DateAdded))]
    [MapperIgnoreSource(nameof(UploadRecordDto.File))]
    public partial MedicalRecord ToEntity(UploadRecordDto dto);
}
