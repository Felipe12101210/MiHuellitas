using MiHuellitas.shared.Enums;

namespace MiHuellitas.shared.Models;

public class Pet
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string Breed { get; set; } = string.Empty;
    public int AgeMonths { get; set; }
    public string Sex { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public ListingType ListingType { get; set; }
    public PetStatus Status { get; set; }
    public string ResponsibleName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;
    public Foundation? Foundation { get; set; }
}