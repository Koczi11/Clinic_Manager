using Clinic_Manager.Models;
using System.ComponentModel.DataAnnotations;

namespace Clinic_Manager.DTOs;

public class UpdateVisitStatusDto
{
    [Required(ErrorMessage = "Status jest wymagany.")]
    public VisitStatus Status { get; set; }
}
