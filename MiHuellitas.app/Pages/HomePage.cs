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
        var root = new VerticalStackLayout { Spacing = 16, Padding = new Thickness(12, 18) };

        // Header
        var header = new HorizontalStackLayout { VerticalOptions = LayoutOptions.Center, Spacing = 12 };
        header.Children.Add(new Image { Source = "dotnet_bot.png", HeightRequest = 48, WidthRequest = 48, Aspect = Aspect.AspectFill });
        header.Children.Add(new Label { Text = "MiHuellitas", FontSize = 20, FontAttributes = FontAttributes.Bold, VerticalOptions = LayoutOptions.Center });
        root.Children.Add(header);

        // Hero
        var hero = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(GridLength.Star), new ColumnDefinition(new GridLength(120)) }, RowDefinitions = new RowDefinitionCollection { new RowDefinition(GridLength.Auto) } };
        var heroText = new VerticalStackLayout { Spacing = 6 };
        heroText.Children.Add(new Label { Text = "Bienestar animal", FontAttributes = FontAttributes.Bold, FontSize = 12 });
        heroText.Children.Add(new Label { Text = "Cuidando cada huella con amor y acción.", FontSize = 18, FontAttributes = FontAttributes.Bold });
        heroText.Children.Add(new Label { Text = "Encuentra animales para adoptar, reporta mascotas perdidas y conecta con fundaciones.", FontSize = 14 });
        var ctaRow = new HorizontalStackLayout { Spacing = 8 };
        ctaRow.Children.Add(new Button { Text = "Adoptar", BackgroundColor = Colors.Green, TextColor = Colors.White, Command = new Command(async () => await Shell.Current.GoToAsync("/adoptions")) });
        ctaRow.Children.Add(new Button { Text = "Campañas", BackgroundColor = Colors.Blue, TextColor = Colors.White, Command = new Command(async () => await Shell.Current.GoToAsync("/campaigns")) });
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
        root.Children.Add(new Label { Text = "Qué puedes hacer hoy", FontAttributes = FontAttributes.Bold, FontSize = 16 });
        var actions = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(), new ColumnDefinition() }, RowSpacing = 8, ColumnSpacing = 8 };
        var a0 = ActionCard("Adopta", "Explora mascotas disponibles", "/adoptions", "https://images.unsplash.com/photo-1543466835-00a7907e9de1?auto=format&fit=crop&w=800&q=80"); actions.Children.Add(a0); Grid.SetColumn(a0, 0); Grid.SetRow(a0, 0);
        var a1 = ActionCard("Perdidos", "Reportes recientes", "/lost", "https://images.unsplash.com/photo-1517849845537-4d257902454a?auto=format&fit=crop&w=800&q=80"); actions.Children.Add(a1); Grid.SetColumn(a1, 1); Grid.SetRow(a1, 0);
        var a2 = ActionCard("Encontrados", "Mascotas rescatadas", "/found", "https://images.unsplash.com/photo-1537151608828-ea2b11777ee8?auto=format&fit=crop&w=800&q=80"); actions.Children.Add(a2); Grid.SetColumn(a2, 0); Grid.SetRow(a2, 1);
        var a3 = ActionCard("Campañas", "Participa en jornadas", "/campaigns", "https://images.unsplash.com/photo-1583337130417-3346a1be7dee?auto=format&fit=crop&w=800&q=80"); actions.Children.Add(a3); Grid.SetColumn(a3, 1); Grid.SetRow(a3, 1);
        root.Children.Add(actions);

        // Featured pets
        root.Children.Add(new Label { Text = "Adopciones destacadas", FontAttributes = FontAttributes.Bold, FontSize = 16 });
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
            root.Children.Add(new ScrollView { Orientation = ScrollOrientation.Horizontal, Content = featured, HeightRequest = 200 });
        }

        // Campaigns
        root.Children.Add(new Label { Text = "Campañas próximas", FontAttributes = FontAttributes.Bold, FontSize = 16 });
        if (_catalog is null || !_catalog.Campaigns.Any())
        {
            root.Children.Add(new Label { Text = "No hay campañas.", HorizontalOptions = LayoutOptions.Center });
        }
        else
        {
            var campList = new VerticalStackLayout { Spacing = 8 };
            foreach (var c in _catalog.Campaigns.Take(3))
            {
                var b = new Border { Stroke = Colors.LightGray, StrokeThickness = 1, Padding = 8 };
                var v = new VerticalStackLayout();
                v.Children.Add(new Label { Text = c.Title, FontAttributes = FontAttributes.Bold });
                v.Children.Add(new Label { Text = $"{c.Location} • {c.EventDate:d}", FontSize = 12 });
                b.Content = v;
                campList.Children.Add(b);
            }
            root.Children.Add(campList);
        }

        // Foundations
        root.Children.Add(new Label { Text = "Fundaciones destacadas", FontAttributes = FontAttributes.Bold, FontSize = 16 });
        if (_catalog is null || !_catalog.Foundations.Any())
        {
            root.Children.Add(new Label { Text = "No hay fundaciones.", HorizontalOptions = LayoutOptions.Center });
        }
        else
        {
            var fList = new HorizontalStackLayout { Spacing = 8 };
            foreach (var f in _catalog.Foundations.Take(4))
            {
                var b = new Border { Stroke = Colors.LightGray, StrokeThickness = 1, Padding = 8 };
                var v = new VerticalStackLayout();
                v.Children.Add(new Label { Text = f.Name, FontAttributes = FontAttributes.Bold });
                v.Children.Add(new Label { Text = f.Location, FontSize = 12 });
                b.Content = v;
                fList.Children.Add(b);
            }
            root.Children.Add(new ScrollView { Orientation = ScrollOrientation.Horizontal, Content = fList, HeightRequest = 120 });
        }

        return new ScrollView { Content = root };
    }

    private View StatBox(string big, string small)
    {
        return new VerticalStackLayout { Spacing = 2, HorizontalOptions = LayoutOptions.FillAndExpand, Children = { new Label { Text = big, FontAttributes = FontAttributes.Bold }, new Label { Text = small, FontSize = 12 } } };
    }

    private View ActionCard(string title, string desc, string route, string image)
    {
        var b = new Border { Stroke = Colors.LightGray, StrokeThickness = 1, Padding = 6 };
        var v = new VerticalStackLayout { Spacing = 6 };
        v.Children.Add(new Image { Source = image, HeightRequest = 80, Aspect = Aspect.AspectFill });
        v.Children.Add(new Label { Text = title, FontAttributes = FontAttributes.Bold });
        v.Children.Add(new Label { Text = desc, FontSize = 12 });
        var btn = new Button { Text = "Ir" };
        btn.Clicked += async (s, e) => await Shell.Current.GoToAsync(route);
        v.Children.Add(btn);
        b.Content = v;
        return b;
    }

    private View PetCard(MiHuellitas.shared.Models.Pet pet)
    {
        var b = new Border { Stroke = Colors.LightGray, StrokeThickness = 1, Padding = 6, WidthRequest = 220 };
        var v = new VerticalStackLayout { Spacing = 6 };
        if (!string.IsNullOrEmpty(pet.ImageUrl)) v.Children.Add(new Image { Source = pet.ImageUrl, HeightRequest = 100, Aspect = Aspect.AspectFill });
        v.Children.Add(new Label { Text = pet.Name, FontAttributes = FontAttributes.Bold });
        v.Children.Add(new Label { Text = $"{pet.Breed} · {pet.Location}", FontSize = 12 });
        var btn = new Button { Text = "Ver" };
        btn.Clicked += async (s, e) => await Shell.Current.GoToAsync($"petdetail?petId={pet.Id}");
        v.Children.Add(btn);
        b.Content = v;
        return b;
    }
}
