using MiHuellitas.shared.Enums;
using MiHuellitas.shared.Models;

namespace MiHuellitas.shared.Data;

public static class MockData
{
    // Geographic bounds of the stylized map (Bogotá, Colombia). Shared with the
    // Web and MAUI map projections so pins land in equivalent places.
    public const double LAT_MIN = 4.55;
    public const double LAT_MAX = 4.75;
    public const double LON_MIN = -74.15;
    public const double LON_MAX = -74.05;

    // Coordinates per pet mapped to Bogotá localities.
    private static (double Lat, double Lon) PetCoords(int id)
    {
        return id switch
        {
            1 => (4.70, -74.08),  // Luna — Usaquén
            2 => (4.67, -74.08),  // Milo — Chapinero
            3 => (4.60, -74.08),  // Nala — La Candelaria
            4 => (4.60, -74.13),  // Robin — Kennedy
            5 => (4.70, -74.12),  // Kira — Suba
            6 => (4.67, -74.12),  // Toby — Fontibón
            _ => (4.65, -74.10)
        };
    }

    public static List<Foundation> Foundations { get; } =
    [
        new Foundation
        {
            Id = 1,
            Name = "Huellitas de Esperanza",
            Description = "Refugio comunitario enfocado en recuperación, adopción responsable y cuidado integral de perros y gatos.",
            Location = "Bogotá, Colombia",
            ContactPhone = "+57 310 456 7890",
            Email = "hola@huellitasdeesperanza.org",
            LogoUrl = "https://images.unsplash.com/photo-1548199973-03cce0bbc87b?auto=format&fit=crop&w=400&q=80",
            IsVerified = true
        },
        new Foundation
        {
            Id = 2,
            Name = "Rescate Vivo",
            Description = "Fundación dedicada a rescates urbanos, campañas de esterilización y rehabilitación animal.",
            Location = "Engativá, Bogotá",
            ContactPhone = "+57 320 998 1122",
            Email = "contacto@rescatevivo.org",
            LogoUrl = "https://images.unsplash.com/photo-1537151608828-ea2b11777ee8?auto=format&fit=crop&w=400&q=80",
            IsVerified = true
        },
        new Foundation
        {
            Id = 3,
            Name = "Casa Sin Huellas",
            Description = "Centro de apoyo temporal sin registro de mascotas actualmente.",
            Location = "Teusaquillo, Bogotá",
            ContactPhone = "+57 320 000 0000",
            Email = "contacto@casasinhuellas.org",
            LogoUrl = "https://images.unsplash.com/photo-1543872084-c7bd3822856f?auto=format&fit=crop&w=400&q=80",
            IsVerified = false
        }
    ];

    public static List<Pet> Pets { get; } =
    [
        new Pet
        {
            Id = 1,
            Name = "Luna",
            Species = "Perro",
            Breed = "Labrador",
            AgeMonths = 24,
            Sex = "Hembra",
            Size = "Mediana",
            Location = "Usaquén, Bogotá",
            Latitude = PetCoords(1).Lat,
            Longitude = PetCoords(1).Lon,
            Description = "Muy sociable, juguetona y excelente compañía para familias activas.",
            ImageUrl = "https://images.unsplash.com/photo-1548199973-03cce0bbc87b?auto=format&fit=crop&w=900&q=80",
            ListingType = ListingType.Adoption,
            Status = PetStatus.Available,
            ResponsibleName = "Huellitas de Esperanza",
            ContactPhone = "+57 300 111 2233",
            LastSeenAt = DateTime.UtcNow.AddDays(-2),
            Foundation = Foundations[0]
        },
        new Pet
        {
            Id = 2,
            Name = "Milo",
            Species = "Perro",
            Breed = "Callejero",
            AgeMonths = 18,
            Sex = "Macho",
            Size = "Pequeño",
            Location = "Chapinero, Bogotá",
            Latitude = PetCoords(2).Lat,
            Longitude = PetCoords(2).Lon,
            Description = "Se adapta bien a departamentos, es atento y muy curioso.",
            ImageUrl = "https://images.unsplash.com/photo-1517849845537-4d257902454a?auto=format&fit=crop&w=900&q=80",
            ListingType = ListingType.Adoption,
            Status = PetStatus.Available,
            ResponsibleName = "Rescate Vivo",
            ContactPhone = "+57 315 456 9012",
            LastSeenAt = DateTime.UtcNow.AddDays(-5),
            Foundation = Foundations[1]
        },
        new Pet
        {
            Id = 3,
            Name = "Nala",
            Species = "Gato",
            Breed = "Criollo",
            AgeMonths = 9,
            Sex = "Hembra",
            Size = "Pequeña",
            Location = "La Candelaria, Bogotá",
            Latitude = PetCoords(3).Lat,
            Longitude = PetCoords(3).Lon,
            Description = "Muy tranquila, elegante y acostumbrada a convivir con niños.",
            ImageUrl = "https://images.unsplash.com/photo-1511044568932-338cba0ad803?auto=format&fit=crop&w=900&q=80",
            ListingType = ListingType.Adoption,
            Status = PetStatus.Available,
            ResponsibleName = "Huellitas de Esperanza",
            ContactPhone = "+57 301 445 7799",
            LastSeenAt = DateTime.UtcNow.AddDays(-1),
            Foundation = Foundations[0]
        },
        new Pet
        {
            Id = 4,
            Name = "Robin",
            Species = "Perro",
            Breed = "Pastor Alemán",
            AgeMonths = 30,
            Sex = "Macho",
            Size = "Grande",
            Location = "Kennedy, Bogotá",
            Latitude = PetCoords(4).Lat,
            Longitude = PetCoords(4).Lon,
            Description = "Fue encontrado cerca de una universidad y aún necesita una familia responsable.",
            ImageUrl = "https://images.unsplash.com/photo-1583337130417-3346a1be7dee?auto=format&fit=crop&w=900&q=80",
            ListingType = ListingType.Found,
            Status = PetStatus.Found,
            ResponsibleName = "Rescate Vivo",
            ContactPhone = "+57 317 439 7781",
            LastSeenAt = DateTime.UtcNow.AddDays(-3),
            Foundation = Foundations[1]
        },
        new Pet
        {
            Id = 5,
            Name = "Kira",
            Species = "Perro",
            Breed = "Beagle",
            AgeMonths = 14,
            Sex = "Hembra",
            Size = "Mediana",
            Location = "Suba, Bogotá",
            Latitude = PetCoords(5).Lat,
            Longitude = PetCoords(5).Lon,
            Description = "Se perdió el pasado sábado cerca de la avenida principal, lleva collar con placa.",
            ImageUrl = "https://images.unsplash.com/photo-1537151627183-2d48c86b98a1?auto=format&fit=crop&w=900&q=80",
            ListingType = ListingType.Lost,
            Status = PetStatus.Missing,
            ResponsibleName = "Daniel García",
            ContactPhone = "+57 302 778 9001",
            LastSeenAt = DateTime.UtcNow.AddDays(-1),
            Foundation = Foundations[0]
        },
        new Pet
        {
            Id = 6,
            Name = "Toby",
            Species = "Perro",
            Breed = "Corgi",
            AgeMonths = 24,
            Sex = "Macho",
            Size = "Pequeño",
            Location = "Fontibón, Bogotá",
            Latitude = PetCoords(6).Lat,
            Longitude = PetCoords(6).Lon,
            Description = "Se reportó como perdido por una familia con bebés; responder a cualquier aviso.",
            ImageUrl = "https://images.unsplash.com/photo-1583511655857-d19b40a7a54e?auto=format&fit=crop&w=900&q=80",
            ListingType = ListingType.Lost,
            Status = PetStatus.Missing,
            ResponsibleName = "María Cardona",
            ContactPhone = "+57 304 998 5566",
            LastSeenAt = DateTime.UtcNow.AddDays(-2),
            Foundation = Foundations[1]
        }
    ];

    public static List<Campaign> Campaigns { get; } =
    [
        new Campaign
        {
            Id = 1,
            Title = "Jornada de adopción familiar",
            Description = "Encuentra la mascota ideal y recibe asesoría para integración temprana en tu hogar.",
            ImageUrl = "https://images.unsplash.com/photo-1548199973-03cce0bbc87b?auto=format&fit=crop&w=900&q=80",
            Location = "Parque Simón Bolívar, Bogotá",
            EventDate = DateTime.UtcNow.AddDays(12),
            Organizer = "Huellitas de Esperanza",
            Status = "Activo"
        },
        new Campaign
        {
            Id = 2,
            Title = "Esterilización comunitaria",
            Description = "Programa gratuito para reducir la reproducción irresponsable y mejorar la salud animal.",
            ImageUrl = "https://images.unsplash.com/photo-1583337130417-3346a1be7dee?auto=format&fit=crop&w=900&q=80",
            Location = "Kennedy, Bogotá",
            EventDate = DateTime.UtcNow.AddDays(20),
            Organizer = "Rescate Vivo",
            Status = "Próximo"
        }
    ];

    public static List<NotificationItem> Notifications { get; } =
    [
        new NotificationItem { Id = 1, Title = "Aviso de adopción", Message = "Luna quedó en espera para entrevista con una familia responsable.", CreatedAt = DateTime.UtcNow.AddHours(-5), Type = "Adopción" },
        new NotificationItem { Id = 2, Title = "Reporte actualizado", Message = "Kira fue vista cerca de la avenida 68, revisa la sección de perdidos.", CreatedAt = DateTime.UtcNow.AddHours(-12), Type = "Reporte" },
        new NotificationItem { Id = 3, Title = "Campaña cercana", Message = "La jornada de adopción del domingo ya está disponible para inscripción.", CreatedAt = DateTime.UtcNow.AddDays(-1), Type = "Campaña" }
    ];

    // Reports submitted by users (stored in-memory for mock)
    public static List<Report> Reports { get; } = new List<Report>();

    // Favorite pet ids for the current user (stored in-memory for mock)
    public static HashSet<int> FavoritePetIds { get; } = new HashSet<int>();

    public static UserProfile CurrentUser { get; } = new UserProfile
    {
        Id = 1,
        Name = "Camila Torres",
        Email = "camila@mihuellitas.com",
        Phone = "+57 310 234 5567",
        City = "Bogotá",
        Bio = "Apasionada por rescatar, apoyar adopciones y compartir información útil para animales en riesgo.",
        AvatarUrl = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=400&q=80"
    };
}
