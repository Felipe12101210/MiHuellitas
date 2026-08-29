using MiHuellitas.shared.Data;
using MiHuellitas.shared.Enums;
using MiHuellitas.shared.Models;

namespace MiHuellitas.shared.Services;

public sealed class MockCatalogService
{
    public IReadOnlyList<Pet> Pets => MockData.Pets;
    public IReadOnlyList<Pet> Adoptions => MockData.Pets.Where(p => p.ListingType == ListingType.Adoption).ToList();
    public IReadOnlyList<Pet> LostPets => MockData.Pets.Where(p => p.ListingType == ListingType.Lost).ToList();
    public IReadOnlyList<Pet> FoundPets => MockData.Pets.Where(p => p.ListingType == ListingType.Found).ToList();
    public IReadOnlyList<Foundation> Foundations => MockData.Foundations;
    public IReadOnlyList<Campaign> Campaigns => MockData.Campaigns;
    public IReadOnlyList<NotificationItem> Notifications => MockData.Notifications;
    public UserProfile CurrentUser => MockData.CurrentUser;

    // Simple in-memory report submission for the mock service
    public Report SubmitReport(Report report)
    {
        // assign id
        var nextId = MockData.Reports.Any() ? MockData.Reports.Max(r => r.Id) + 1 : 1;
        report.Id = nextId;
        report.CreatedAt = DateTime.UtcNow;
        MockData.Reports.Add(report);

        // Optionally add a notification to simulate background processing
        MockData.Notifications.Insert(0, new NotificationItem
        {
            Id = MockData.Notifications.Any() ? MockData.Notifications.Max(n => n.Id) + 1 : 1,
            Title = "Nuevo reporte",
            Message = $"Reporte para mascota ID {report.PetId} recibido: {report.Type}",
            CreatedAt = DateTime.UtcNow,
            Type = "Reporte"
        });

        return report;
    }
}
