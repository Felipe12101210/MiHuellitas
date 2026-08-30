using Microsoft.Maui.Controls;
using MiHuellitas.shared.Services;
using System.Linq;

namespace MiHuellitas.app.Pages;

public class FoundPetsPage : ContentPage
{
    private MockCatalogService? _catalog;
    private VerticalStackLayout? _itemsContainer;

    public FoundPetsPage()
    {
        Title = "Encontrados";
        _catalog = MauiProgram.Services?.GetService(typeof(MockCatalogService)) as MockCatalogService;
        var layout = new VerticalStackLayout { Padding = 12, Spacing = 10 };

        if (_catalog is null)
        {
            layout.Children.Add(new Label { Text = "Servicio no disponible" });
        }
        else
        {
            _itemsContainer = new VerticalStackLayout { Spacing = 8 };
            BuildList();
            layout.Children.Add(new ScrollView { Content = _itemsContainer });

            _catalog.PetsChanged += OnPetsChanged;
        }

        Content = layout;
    }

    private void BuildList()
    {
        if (_itemsContainer is null || _catalog is null) return;
        _itemsContainer.Children.Clear();

        if (!_catalog.FoundPets.Any())
        {
            _itemsContainer.Children.Add(new Label { Text = "No hay mascotas encontradas.", HorizontalOptions = LayoutOptions.Center });
            return;
        }

        foreach (var p in _catalog.FoundPets)
        {
            var border = new Border { Padding = 8, Stroke = Colors.LightGray, StrokeThickness = 1 };
            var v = new VerticalStackLayout();
            v.Children.Add(new Label { Text = p.Name, FontAttributes = FontAttributes.Bold });
            v.Children.Add(new Label { Text = $"{p.LastSeenAt:g} • {p.Location}", FontSize = 12 });
            var btn = new Button { Text = "Ver detalle" };
            btn.Clicked += async (s, e) => await Shell.Current.GoToAsync($"petdetail?petId={p.Id}");
            v.Children.Add(btn);
            border.Content = v;
            _itemsContainer.Children.Add(border);
        }
    }

    private void OnPetsChanged()
    {
        MainThread.BeginInvokeOnMainThread(BuildList);
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (_catalog is not null) _catalog.PetsChanged -= OnPetsChanged;
    }
}
