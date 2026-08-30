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

    // Campaign helpers
    public IReadOnlyList<Campaign> GetCampaigns() => Campaigns;

    public Campaign? GetCampaignById(int id) => MockData.Campaigns.FirstOrDefault(c => c.Id == id);
    public IReadOnlyList<NotificationItem> Notifications => MockData.Notifications;
    public IReadOnlyList<Report> Reports => MockData.Reports;
    public UserProfile CurrentUser => MockData.CurrentUser;

    // Events to notify UI about changes in notifications/reports/pets (mock reactive)
    public event Action? NotificationsChanged;
    public event Action? PetsChanged;
    public event Action? FavoritesChanged;

    // Convenience methods for Foundations
    public IReadOnlyList<Foundation> GetFoundations() => Foundations;

    // Add a pet to the in-memory store through the service (assigns Id and notifies subscribers)
    public Pet AddPet(Pet pet)
    {
        var nextId = MockData.Pets.Any() ? MockData.Pets.Max(p => p.Id) + 1 : 1;
        pet.Id = nextId;
        // If LastSeenAt not set and listing is Lost/Found, set to now; otherwise keep default
        if ((pet.ListingType == ListingType.Lost || pet.ListingType == ListingType.Found) && pet.LastSeenAt == default)
        {
            pet.LastSeenAt = DateTime.UtcNow;
        }

        MockData.Pets.Add(pet);

        // Notify subscribers that pets changed
        try
        {
            PetsChanged?.Invoke();
        }
        catch { }

        return pet;
    }

    // Notification management helpers
    public void MarkNotificationRead(int id)
    {
        var n = MockData.Notifications.FirstOrDefault(x => x.Id == id);
        if (n is not null)
        {
            n.IsRead = true;
            try { NotificationsChanged?.Invoke(); } catch { }
        }
    }

    public void RemoveNotification(int id)
    {
        var n = MockData.Notifications.FirstOrDefault(x => x.Id == id);
        if (n is not null)
        {
            MockData.Notifications.Remove(n);
            try { NotificationsChanged?.Invoke(); } catch { }
        }
    }

    public Foundation? GetFoundationById(int id) => MockData.Foundations.FirstOrDefault(f => f.Id == id);

    public IReadOnlyList<Pet> GetPetsByFoundation(int foundationId)
    {
        return MockData.Pets.Where(p => p.Foundation is not null && p.Foundation.Id == foundationId).ToList();
    }

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

        // Notify subscribers (UI) that notifications / reports have changed
        try
        {
            NotificationsChanged?.Invoke();
        }
        catch
        {
            // best-effort: swallow exceptions from subscribers to avoid breaking mock flow
        }

        return report;
    }

    // Favorites (in-memory mock for the current user)
    public bool IsFavorite(int petId) => MockData.FavoritePetIds.Contains(petId);

    public int FavoritesCount => MockData.FavoritePetIds.Count;

    public IReadOnlyList<Pet> GetFavoritedPets() => MockData.Pets.Where(p => MockData.FavoritePetIds.Contains(p.Id)).ToList();

    public void ToggleFavorite(int petId)
    {
        if (!MockData.FavoritePetIds.Add(petId))
        {
            MockData.FavoritePetIds.Remove(petId);
        }
        try { FavoritesChanged?.Invoke(); } catch { }
    }
}
