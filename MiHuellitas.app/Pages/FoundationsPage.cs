using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using MiHuellitas.shared.Services;
using System.Linq;

namespace MiHuellitas.app.Pages;

public class FoundationsPage : ContentPage
{
    private MockCatalogService? _catalog;
    private VerticalStackLayout? _itemsContainer;
    private string _searchName = string.Empty;
    private string _searchLocation = string.Empty;

    public FoundationsPage()
    {
        Title = "Fundaciones";
        _catalog = MauiProgram.Services?.GetService(typeof(MockCatalogService)) as MockCatalogService;

        // Loading state is intentionally omitted: the mock catalog is a synchronous,
        // in-memory service, so there is no async fetch to wait for.
        var layout = new VerticalStackLayout { Padding = new Thickness(14, 18, 14, 0), Spacing = 12 };

        if (_catalog is null)
        {
            layout.Children.Add(new Label { Text = "Ocurrió un error al cargar las fundaciones. Intenta más tarde." });
        }
        else
        {
            layout.Children.Add(new Label { Text = "Comunidad", Style = Res("Eyebrow") });
            layout.Children.Add(new Label { Text = "Fundaciones", Style = Res("PageTitle") });

            var filters = new VerticalStackLayout { Spacing = 8 };

            filters.Children.Add(new Label { Text = "Buscar por nombre", Style = Res("MutedText") });
            var nameEntry = new Entry { Placeholder = "Buscar por nombre" };
            nameEntry.TextChanged += (s, e) => { _searchName = e.NewTextValue ?? string.Empty; BuildList(); };
            filters.Children.Add(nameEntry);

            filters.Children.Add(new Label { Text = "Buscar por ubicación", Style = Res("MutedText") });
            var locationEntry = new Entry { Placeholder = "Buscar por ubicación" };
            locationEntry.TextChanged += (s, e) => { _searchLocation = e.NewTextValue ?? string.Empty; BuildList(); };
            filters.Children.Add(locationEntry);

            var clearBtn = new Button { Text = "Limpiar", Style = Res("SecondaryButton") };
            clearBtn.Clicked += (s, e) =>
            {
                _searchName = string.Empty;
                _searchLocation = string.Empty;
                nameEntry.Text = string.Empty;
                locationEntry.Text = string.Empty;
                BuildList();
            };
            filters.Children.Add(clearBtn);

            _itemsContainer = new VerticalStackLayout { Spacing = 8 };
            var filtersPanel = new Border { Padding = 12, Content = filters };
            layout.Children.Add(filtersPanel);
            layout.Children.Add(_itemsContainer);

            BuildList();
        }

        Content = new ScrollView { Content = layout };
    }

    private void BuildList()
    {
        if (_itemsContainer is null || _catalog is null) return;
        _itemsContainer.Children.Clear();

        var filtered = _catalog.Foundations
            .Where(f => string.IsNullOrEmpty(_searchName) || f.Name.Contains(_searchName, StringComparison.OrdinalIgnoreCase))
            .Where(f => string.IsNullOrEmpty(_searchLocation) || f.Location.Contains(_searchLocation, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!filtered.Any())
        {
            _itemsContainer.Children.Add(new Label { Text = "No se encontraron fundaciones que coincidan con los filtros.", HorizontalOptions = LayoutOptions.Center });
            return;
        }

        foreach (var f in filtered)
        {
            var border = new Border { Padding = 12 };
            var v = new VerticalStackLayout { Spacing = 10 };

            var header = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(new GridLength(80)),
                    new ColumnDefinition(GridLength.Star)
                },
                ColumnSpacing = 12
            };

            var logo = new Border
            {
                WidthRequest = 80,
                HeightRequest = 80,
                Padding = 0,
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                BackgroundColor = (Color)Application.Current!.Resources["Surface"],
                Content = (!string.IsNullOrEmpty(f.LogoUrl))
                    ? new Image { Source = f.LogoUrl, Aspect = Aspect.AspectFill }
                    : new Label { Text = "🐾", FontSize = 34, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center }
            };
            header.Children.Add(logo);

            var headerText = new VerticalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Center };
            headerText.Children.Add(new Label { Text = f.Name, Style = Res("CardTitle") });
            if (f.IsVerified)
                headerText.Children.Add(new Label { Text = "Verificada", Style = Res("BadgeTextAccent") });
            headerText.Children.Add(new Label { Text = f.Location, Style = Res("MutedText") });
            header.Children.Add(headerText);
            Grid.SetColumn(headerText, 1);

            v.Children.Add(header);

            if (!string.IsNullOrEmpty(f.Description))
                v.Children.Add(new Label { Text = f.Description, Style = Res("MutedText") });
            var petCount = _catalog.GetPetsByFoundation(f.Id).Count();
            v.Children.Add(new Label { Text = $"{petCount} mascotas registradas", Style = Res("MutedText") });
            v.Children.Add(new Label { Text = f.ContactPhone, Style = Res("MutedText") });
            v.Children.Add(new Label { Text = f.Email, Style = Res("MutedText") });
            var btn = new Button { Text = "Ver detalle", Style = Res("SecondaryButton") };
            btn.Clicked += async (s, e) => await SafeGoToAsync($"foundationdetail?foundationId={f.Id}");
            v.Children.Add(btn);

            var tap = new TapGestureRecognizer();
            tap.Tapped += async (s, e) => await SafeGoToAsync($"foundationdetail?foundationId={f.Id}");
            v.GestureRecognizers.Add(tap);

            border.Content = v;
            _itemsContainer.Children.Add(border);
        }
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
