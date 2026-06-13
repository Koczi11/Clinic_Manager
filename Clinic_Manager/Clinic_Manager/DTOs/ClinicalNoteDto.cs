namespace Clinic_Manager.DTOs;

public class ClinicalNoteDto
{
    public int Id { get; set; }
    public int VisitId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime DateCreated { get; set; }
    public string AuthorName { get; set; } = string.Empty;
}
