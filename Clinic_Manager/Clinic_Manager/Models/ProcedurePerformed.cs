using System.ComponentModel.DataAnnotations;

namespace Clinic_Manager.Models;

public class ProcedurePerformed
{
    public int Id { get; set; }

    [Required]
    public int VisitId { get; set; }
    public Visit Visit { get; set; } = null!;

    [Required]
    public int ProcedureId { get; set; }
    public Procedure Procedure { get; set; } = null!;
}
