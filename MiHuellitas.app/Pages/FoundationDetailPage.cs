using Microsoft.Maui.Controls;
using MiHuellitas.shared.Services;
using System;
using System.Linq;

namespace MiHuellitas.app.Pages;

[QueryProperty(nameof(FoundationId), "foundationId")]
public class FoundationDetailPage : ContentPage
{
    private MockCatalogService? _catalog;
    private int _foundationId;

    public string FoundationId
    {
        set
        {
            if (int.TryParse(value, out var id))
            {
                _foundationId = id;
                LoadFoundation();
            }
        }
    }

    public FoundationDetailPage()
    {
        Title = "Detalle fundación";
        _catalog = MauiProgram.Services?.GetService(typeof(MockCatalogService)) as MockCatalogService;
        Content = new VerticalStackLayout { Padding = 12, Children = { new Label { Text = "Cargando..." } } };
    }

    private void LoadFoundation()
    {
        if (_catalog is null)
        {
            Content = new Label { Text = "Servicio no disponible" };
            return;
        }

        var foundation = _catalog.GetFoundationById(_foundationId);
        if (foundation is null)
        {
            Content = new Label { Text = "Fundación no encontrada." };
            return;
        }

        var layout = new VerticalStackLayout { Padding = 12, Spacing = 8 };
        layout.Children.Add(new Label { Text = "Fundación", Style = Res("Eyebrow") });
        if (!string.IsNullOrEmpty(foundation.LogoUrl))
            layout.Children.Add(new Image { Source = foundation.LogoUrl, HeightRequest = 200, Aspect = Aspect.AspectFill });
        layout.Children.Add(new Label { Text = foundation.Name, Style = Res("PageTitle") });
        if (foundation.IsVerified)
            layout.Children.Add(new Label { Text = "Verificada", Style = Res("BadgeTextAccent") });
        layout.Children.Add(new Label { Text = $"Ubicación: {foundation.Location}", Style = Res("MutedText") });
        layout.Children.Add(new Label { Text = foundation.Description, Style = Res("MutedText") });
        layout.Children.Add(new Label { Text = $"Tel: {foundation.ContactPhone}", Style = Res("MutedText") });
        layout.Children.Add(new Label { Text = $"Email: {foundation.Email}", Style = Res("MutedText") });

        var pets = _catalog.GetPetsByFoundation(foundation.Id).ToList();
        layout.Children.Add(new Label { Text = $"Mascotas asociadas ({pets.Count})", Style = Res("SectionTitle") });

        if (!pets.Any())
        {
            layout.Children.Add(new Label { Text = "Esta fundación actualmente no tiene animales registrados.", Style = Res("MutedText") });
        }
        else
        {
            foreach (var p in pets)
            {
                var border = new Border { Padding = 10 };
                var v = new VerticalStackLayout { Spacing = 6 };
                v.Children.Add(new Label { Text = p.Name, Style = Res("CardTitle") });
                v.Children.Add(new Label { Text = $"{p.Species} • {p.Breed} • {p.Location}", Style = Res("MutedText") });
                var btn = new Button { Text = "Ver detalle", Style = Res("SecondaryButton") };
                btn.Clicked += async (s, e) => await SafeGoToAsync($"petdetail?petId={p.Id}");
                v.Children.Add(btn);
                border.Content = v;
                layout.Children.Add(border);
            }
        }

        var backBtn = new Button { Text = "Volver", Style = Res("SecondaryButton") };
        backBtn.Clicked += async (s, e) => await GoBackAsync();
        layout.Children.Add(backBtn);

        Content = new ScrollView { Content = layout };
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

    private async Task GoBackAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GoBack failed: {ex.Message}");
            await Navigation.PopAsync();
        }
    }
}
