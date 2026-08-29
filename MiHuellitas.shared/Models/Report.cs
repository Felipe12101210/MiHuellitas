namespace MiHuellitas.shared.Models;

public class Report
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public string ReporterPhone { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // e.g., "Interest", "Sighted", "Issue"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}