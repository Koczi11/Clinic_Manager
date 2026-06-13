namespace Clinic_Manager.DTOs;

public class ProcedurePerformedDto
{
    public int Id { get; set; }
    public int VisitId { get; set; }
    public int ProcedureId { get; set; }
    public string ProcedureName { get; set; } = string.Empty;
    public decimal Cost { get; set; }
}
