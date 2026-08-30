using Microsoft.Maui.Controls;
using MiHuellitas.shared.Services;
using System.Linq;

namespace MiHuellitas.app.Pages;

public class HomePage : ContentPage
{
    private readonly MockCatalogService? _catalog;

    public HomePage()
    {
        Title = "MiHuellitas";
        _catalog = MauiProgram.Services?.GetService(typeof(MockCatalogService)) as MockCatalogService;

        Content = BuildContent();
    }

    private View BuildContent()
    {
        // Loading state is intentionally omitted: the mock catalog is a synchronous,
        // in-memory service, so there is no async fetch to wait for.
        var root = new VerticalStackLayout { Spacing = 16, Padding = new Thickness(12, 18) };

        // Header
        var header = new HorizontalStackLayout { VerticalOptions = LayoutOptions.Center, Spacing = 12 };
        header.Children.Add(new Image { Source = "dotnet_bot.png", HeightRequest = 48, WidthRequest = 48, Aspect = Aspect.AspectFill });
        header.Children.Add(new Label { Text = "MiHuellitas", FontSize = 20, FontAttributes = FontAttributes.Bold, VerticalOptions = LayoutOptions.Center });
        root.Children.Add(header);

        // Hero
        var hero = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(GridLength.Star), new ColumnDefinition(new GridLength(120)) }, RowDefinitions = new RowDefinitionCollection { new RowDefinition(GridLength.Auto) } };
        var heroText = new VerticalStackLayout { Spacing = 6 };
        heroText.Children.Add(new Label { Text = "Bienestar animal", Style = Res("Eyebrow") });
        heroText.Children.Add(new Label { Text = "Cuidando cada huella con amor y acción.", FontSize = 22, FontAttributes = FontAttributes.Bold });
        heroText.Children.Add(new Label { Text = "Encuentra animales para adoptar, reporta mascotas perdidas y conecta con fundaciones.", Style = Res("MutedText"), FontSize = 14 });
        var ctaRow = new HorizontalStackLayout { Spacing = 8 };
        ctaRow.Children.Add(new Button { Text = "Adoptar", Command = new Command(async () => await SafeGoToAsync("//adoptions")) });
        ctaRow.Children.Add(new Button { Text = "Campañas", Style = Res("SecondaryButton"), Command = new Command(async () => await SafeGoToAsync("//campaigns")) });
        heroText.Children.Add(ctaRow);
        // place heroText in column 0
        hero.Children.Add(heroText);
        Grid.SetColumn(heroText, 0);

        var heroImage = new Image { Source = "https://images.unsplash.com/photo-1543466835-00a7907e9de1?auto=format&fit=crop&w=600&q=80", Aspect = Aspect.AspectFill, HeightRequest = 120 };
        hero.Children.Add(heroImage);
        Grid.SetColumn(heroImage, 1);
        Grid.SetRow(heroImage, 0);
        root.Children.Add(hero);

        // Stats strip
        var stats = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(), new ColumnDefinition(), new ColumnDefinition(), new ColumnDefinition() }, ColumnSpacing = 8 };
        var s0 = StatBox("120+", "Casos atendidos"); stats.Children.Add(s0); Grid.SetColumn(s0, 0); Grid.SetRow(s0, 0);
        var s1 = StatBox("40", "Fundaciones"); stats.Children.Add(s1); Grid.SetColumn(s1, 1); Grid.SetRow(s1, 0);
        var s2 = StatBox("18", "Campañas"); stats.Children.Add(s2); Grid.SetColumn(s2, 2); Grid.SetRow(s2, 0);
        var s3 = StatBox("96%", "Seguimiento"); stats.Children.Add(s3); Grid.SetColumn(s3, 3); Grid.SetRow(s3, 0);
        root.Children.Add(stats);

        // Quick actions
        root.Children.Add(new Label { Text = "Qué puedes hacer hoy", Style = Res("SectionTitle") });
        var actions = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(), new ColumnDefinition() }, RowSpacing = 8, ColumnSpacing = 8 };
        var a0 = ActionCard("Adopta", "Explora mascotas disponibles", "//adoptions", "https://images.unsplash.com/photo-1543466835-00a7907e9de1?auto=format&fit=crop&w=800&q=80"); actions.Children.Add(a0); Grid.SetColumn(a0, 0); Grid.SetRow(a0, 0);
        var a1 = ActionCard("Perdidos", "Reportes recientes", "//lost", "https://images.unsplash.com/photo-1517849845537-4d257902454a?auto=format&fit=crop&w=800&q=80"); actions.Children.Add(a1); Grid.SetColumn(a1, 1); Grid.SetRow(a1, 0);
        var a2 = ActionCard("Encontrados", "Mascotas rescatadas", "//found", "https://images.unsplash.com/photo-1537151608828-ea2b11777ee8?auto=format&fit=crop&w=800&q=80"); actions.Children.Add(a2); Grid.SetColumn(a2, 0); Grid.SetRow(a2, 1);
        var a3 = ActionCard("Campañas", "Participa en jornadas", "//campaigns", "https://images.unsplash.com/photo-1583337130417-3346a1be7dee?auto=format&fit=crop&w=800&q=80"); actions.Children.Add(a3); Grid.SetColumn(a3, 1); Grid.SetRow(a3, 1);
        root.Children.Add(actions);

        // Featured pets
        root.Children.Add(new Label { Text = "Adopciones destacadas", Style = Res("SectionTitle") });
        var featured = new HorizontalStackLayout { Spacing = 8 };
        if (_catalog is null || !_catalog.Adoptions.Any())
        {
            root.Children.Add(new Label { Text = "No hay adopciones destacadas.", HorizontalOptions = LayoutOptions.Center });
        }
        else
        {
            foreach (var pet in _catalog.Adoptions.Take(3))
            {
                featured.Children.Add(PetCard(pet));
            }
            root.Children.Add(new ScrollView { Orientation = ScrollOrientation.Horizontal, Content = featured, HeightRequest = 240 });
        }

        // Campaigns
        root.Children.Add(new Label { Text = "Campañas próximas", Style = Res("SectionTitle") });
        if (_catalog is null || !_catalog.Campaigns.Any())
        {
            root.Children.Add(new Label { Text = "No hay campañas.", HorizontalOptions = LayoutOptions.Center });
        }
        else
        {
            var campList = new VerticalStackLayout { Spacing = 10 };
            foreach (var c in _catalog.Campaigns.Take(3))
            {
                var b = new Border { Padding = 0 };
                var v = new VerticalStackLayout { Spacing = 8 };
                if (!string.IsNullOrEmpty(c.ImageUrl))
                    v.Children.Add(new Image { Source = c.ImageUrl, HeightRequest = 110, Aspect = Aspect.AspectFill });
                var info = new VerticalStackLayout { Spacing = 6, Padding = new Thickness(12, 6, 12, 12) };
                info.Children.Add(new Label { Text = c.Title, Style = Res("CardTitle") });
                info.Children.Add(new Label { Text = c.Organizer, Style = Res("MutedText") });
                info.Children.Add(new Label { Text = $"{c.Location} • {c.EventDate:dd MMM yyyy}", Style = Res("MutedText") });
                info.Children.Add(new Label { Text = c.Status, Style = Res("BadgeTextAccent") });
                v.Children.Add(info);

                var tap = new TapGestureRecognizer();
                tap.Tapped += async (s, e) => await SafeGoToAsync($"campaigndetail?campaignId={c.Id}");
                v.GestureRecognizers.Add(tap);
                b.Content = v;
                campList.Children.Add(b);
            }
            root.Children.Add(campList);
        }

        // Foundations
        root.Children.Add(new Label { Text = "Fundaciones destacadas", Style = Res("SectionTitle") });
        if (_catalog is null || !_catalog.Foundations.Any())
        {
            root.Children.Add(new Label { Text = "No hay fundaciones.", HorizontalOptions = LayoutOptions.Center });
        }
        else
        {
            var fList = new HorizontalStackLayout { Spacing = 10 };
            foreach (var f in _catalog.Foundations.Take(4))
            {
                var b = new Border { Padding = 0, WidthRequest = 180 };
                var v = new VerticalStackLayout { Spacing = 8 };
                if (!string.IsNullOrEmpty(f.LogoUrl))
                    v.Children.Add(new Image { Source = f.LogoUrl, HeightRequest = 80, Aspect = Aspect.AspectFill });
                var info = new VerticalStackLayout { Spacing = 4, Padding = new Thickness(12, 4, 12, 12) };
                info.Children.Add(new Label { Text = f.Name, Style = Res("CardTitle") });
                if (f.IsVerified)
                    info.Children.Add(new Label { Text = "Verificada", Style = Res("BadgeTextAccent") });
                info.Children.Add(new Label { Text = f.Location, Style = Res("MutedText") });
                info.Children.Add(new Label { Text = $"{_catalog.GetPetsByFoundation(f.Id).Count()} mascotas", Style = Res("MutedText") });
                v.Children.Add(info);

                var tap = new TapGestureRecognizer();
                tap.Tapped += async (s, e) => await SafeGoToAsync($"foundationdetail?foundationId={f.Id}");
                v.GestureRecognizers.Add(tap);
                b.Content = v;
                fList.Children.Add(b);
            }
            root.Children.Add(new ScrollView { Orientation = ScrollOrientation.Horizontal, Content = fList, HeightRequest = 230 });
        }

        return new ScrollView { Content = root };
    }

    private View StatBox(string big, string small)
    {
        var b = new Border { Padding = 10, StrokeThickness = 0, BackgroundColor = (Color)Application.Current!.Resources["SurfaceSoft"] };
        var v = new VerticalStackLayout { Spacing = 2, HorizontalOptions = LayoutOptions.Center, Children = { new Label { Text = big, FontAttributes = FontAttributes.Bold, FontSize = 20 }, new Label { Text = small, Style = Res("MutedText") } } };
        b.Content = v;
        return b;
    }

    private View ActionCard(string title, string desc, string route, string image)
    {
        var b = new Border { Padding = 12 };
        var v = new VerticalStackLayout { Spacing = 8 };
        v.Children.Add(new Image { Source = image, HeightRequest = 80, Aspect = Aspect.AspectFill });
        v.Children.Add(new Label { Text = title, Style = Res("CardTitle") });
        v.Children.Add(new Label { Text = desc, Style = Res("MutedText") });
        var btn = new Button { Text = "Ir", Style = Res("SecondaryButton") };
        btn.Clicked += async (s, e) => await SafeGoToAsync(route);
        v.Children.Add(btn);
        b.Content = v;
        return b;
    }

    private View PetCard(MiHuellitas.shared.Models.Pet pet)
    {
        var b = new Border { Padding = 12, WidthRequest = 220 };
        var v = new VerticalStackLayout { Spacing = 8 };
        if (!string.IsNullOrEmpty(pet.ImageUrl)) v.Children.Add(new Image { Source = pet.ImageUrl, HeightRequest = 90, Aspect = Aspect.AspectFill });
        v.Children.Add(new Label { Text = pet.Name, Style = Res("CardTitle") });
        v.Children.Add(new Label { Text = $"{pet.Breed} · {pet.Location}", Style = Res("MutedText") });
        var btn = new Button { Text = "Ver", Style = Res("SecondaryButton") };
        btn.Clicked += async (s, e) => await SafeGoToAsync($"petdetail?petId={pet.Id}");
        v.Children.Add(btn);
        b.Content = v;
        return b;
    }

    private static Style Res(string key) => (Style)Application.Current!.Resources[key];

    private async Task SafeGoToAsync(string route)
    {
        try
        {
            await Shell.Current.GoToAsync(route);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation failed ({route}): {ex.Message}");
        }
    }
}
