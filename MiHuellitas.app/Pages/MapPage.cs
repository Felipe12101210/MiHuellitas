using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using MiHuellitas.shared.Data;
using MiHuellitas.shared.Models;
using MiHuellitas.shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MiHuellitas.app.Pages;

public class MapPage : ContentPage
{
    private MockCatalogService? _catalog;
    private AbsoluteLayout? _mapSurface;
    private VerticalStackLayout? _listContainer;
    private ScrollView? _pageScroll;
    private Border? _infoPanel;
    private VerticalStackLayout? _infoContent;
    private Picker? _speciesFilter;
    private int _backdropCount;
    private string _speciesFilterValue = string.Empty;
    private Pet? _selectedPet;
    private readonly Dictionary<int, Border> _pinDots = new();
    private readonly Dictionary<int, Label> _pinNames = new();
    private readonly Dictionary<int, Border> _rows = new();

    public MapPage()
    {
        Title = "Mapa";
        _catalog = MauiProgram.Services?.GetService(typeof(MockCatalogService)) as MockCatalogService;

        var layout = new VerticalStackLayout { Padding = 12, Spacing = 10 };
        layout.Children.Add(new Label { Text = "Descubre", Style = Res("Eyebrow") });
        layout.Children.Add(new Label { Text = "Mapa", Style = Res("PageTitle") });
        layout.Children.Add(new Label { Text = "Ubicaciones aproximadas · mapa ilustrativo", Style = Res("MutedText") });

        if (_catalog is null)
        {
            Content = new ScrollView { Content = layout };
            return;
        }

        // Species filter
        var filterRow = new VerticalStackLayout { Spacing = 6 };
        filterRow.Children.Add(new Label { Text = "Filtrar por especie", Style = Res("MutedText"), FontSize = 12 });
        _speciesFilter = new Picker { Title = "Todas" };
        _speciesFilter.Items.Add("Todas");
        _speciesFilter.Items.Add("Perro");
        _speciesFilter.Items.Add("Gato");
        _speciesFilter.SelectedIndex = 0;
        _speciesFilter.SelectedIndexChanged += (s, e) =>
        {
            _speciesFilterValue = SpeciesFilterValue();
            _selectedPet = null;
            RebuildPins();
            RenderInfo();
            RebuildList();
        };
        filterRow.Children.Add(_speciesFilter);
        layout.Children.Add(filterRow);

        _mapSurface = new AbsoluteLayout { HeightRequest = 460, BackgroundColor = MapColor("#EFF4F5") };
        BuildBackdrop();
        _backdropCount = _mapSurface.Children.Count;
        _infoPanel = new Border { Padding = 12, IsVisible = false };
        _infoContent = new VerticalStackLayout { Spacing = 6 };
        _infoPanel.Content = _infoContent;
        RebuildPins();
        layout.Children.Add(_mapSurface);

        layout.Children.Add(BuildLegend());

        _listContainer = new VerticalStackLayout { Spacing = 8 };
        _pageScroll = new ScrollView { Content = layout };
        Content = _pageScroll;

        RebuildList();
    }

    private void BuildBackdrop()
    {
        if (_mapSurface is null) return;

        // Main park (top-right) and river (bottom-left) - preserved
        var park = new BoxView { Color = MapColor("#CDE5CF"), CornerRadius = 10 };
        AbsoluteLayout.SetLayoutBounds(park, new Rect(0.64, 0.14, 0.28, 0.20));
        AbsoluteLayout.SetLayoutFlags(park, AbsoluteLayoutFlags.All);
        _mapSurface.Children.Add(park);

        var river = new BoxView { Color = MapColor("#EAF4FB"), CornerRadius = 10 };
        AbsoluteLayout.SetLayoutBounds(river, new Rect(0.12, 0.56, 0.20, 0.32));
        AbsoluteLayout.SetLayoutFlags(river, AbsoluteLayoutFlags.All);
        _mapSurface.Children.Add(river);

        // Minor park (top-left) and central plaza
        var minorPark = new BoxView { Color = MapColor("#D8EED9"), CornerRadius = 6 };
        AbsoluteLayout.SetLayoutBounds(minorPark, new Rect(0.05, 0.05, 0.12, 0.09));
        AbsoluteLayout.SetLayoutFlags(minorPark, AbsoluteLayoutFlags.All);
        _mapSurface.Children.Add(minorPark);

        var plaza = new BoxView { Color = MapColor("#FDF1E7"), CornerRadius = 8 };
        AbsoluteLayout.SetLayoutBounds(plaza, new Rect(0.46, 0.44, 0.13, 0.11));
        AbsoluteLayout.SetLayoutFlags(plaza, AbsoluteLayoutFlags.All);
        _mapSurface.Children.Add(plaza);
        var plazaLabel = new Label { Text = "Plaza", FontSize = 8, TextColor = MapColor("#B98A63"), FontAttributes = FontAttributes.Italic };
        AbsoluteLayout.SetLayoutBounds(plazaLabel, new Rect(0.50, 0.44, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
        AbsoluteLayout.SetLayoutFlags(plazaLabel, AbsoluteLayoutFlags.XProportional);
        _mapSurface.Children.Add(plazaLabel);

        // City blocks between streets (cartilla de ciudad)
        PlaceCityBlocks();

        // Horizontal streets (white bands) with small names
        string[] hNames = { "Calle 63", "Calle 45", "Calle 26", "Calle 13" };
        double[] hStreets = { 0.20, 0.42, 0.64, 0.86 };
        for (int i = 0; i < hStreets.Length; i++)
        {
            var road = new BoxView { Color = Colors.White };
            AbsoluteLayout.SetLayoutBounds(road, new Rect(0, hStreets[i], 1, 0.018));
            AbsoluteLayout.SetLayoutFlags(road, AbsoluteLayoutFlags.All);
            _mapSurface.Children.Add(road);

            var label = new Label { Text = hNames[i], FontSize = 9, TextColor = MapColor("#9AA6AD"), FontAttributes = FontAttributes.Italic };
            AbsoluteLayout.SetLayoutBounds(label, new Rect(10, hStreets[i], AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
            AbsoluteLayout.SetLayoutFlags(label, AbsoluteLayoutFlags.YProportional);
            _mapSurface.Children.Add(label);
        }

        // Vertical avenues (white bands) with small names
        string[] vNames = { "Av. Caracas", "Cra 7", "Cra 13", "Av. 68" };
        double[] vStreets = { 0.22, 0.46, 0.70, 0.90 };
        for (int i = 0; i < vStreets.Length; i++)
        {
            var road = new BoxView { Color = Colors.White };
            AbsoluteLayout.SetLayoutBounds(road, new Rect(vStreets[i], 0, 0.016, 1));
            AbsoluteLayout.SetLayoutFlags(road, AbsoluteLayoutFlags.All);
            _mapSurface.Children.Add(road);

            var label = new Label { Text = vNames[i], FontSize = 9, TextColor = MapColor("#9AA6AD"), FontAttributes = FontAttributes.Italic };
            AbsoluteLayout.SetLayoutBounds(label, new Rect(vStreets[i], 8, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
            AbsoluteLayout.SetLayoutFlags(label, AbsoluteLayoutFlags.XProportional);
            _mapSurface.Children.Add(label);
        }

        // Diagonal streets (rotated white bands) crossing the surface
        AddDiagonal(30, 0.30, "Diag. 50");
        AddDiagonal(-30, 0.62, "Av. Jiménez");
    }

    private void AddDiagonal(double rotation, double yCenter, string name)
    {
        if (_mapSurface is null) return;
        var band = new BoxView { Color = Colors.White, Rotation = rotation };
        AbsoluteLayout.SetLayoutBounds(band, new Rect(0, yCenter, 1, 0.016));
        AbsoluteLayout.SetLayoutFlags(band, AbsoluteLayoutFlags.All);
        _mapSurface.Children.Add(band);

        var label = new Label
        {
            Text = name,
            FontSize = 9,
            TextColor = MapColor("#9AA6AD"),
            FontAttributes = FontAttributes.Italic,
            Rotation = rotation
        };
        AbsoluteLayout.SetLayoutBounds(label, new Rect(0.03, yCenter, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
        AbsoluteLayout.SetLayoutFlags(label, AbsoluteLayoutFlags.All);
        _mapSurface.Children.Add(label);
    }

    private void PlaceCityBlocks()
    {
        if (_mapSurface is null) return;
        double[][] rows = { new[] { 0.22, 0.42 }, new[] { 0.44, 0.64 }, new[] { 0.66, 0.86 } };
        double[][] cols = { new[] { 0.238, 0.46 }, new[] { 0.478, 0.70 }, new[] { 0.718, 0.90 } };
        foreach (var row in rows)
        {
            double rTop = row[0], rBot = row[1];
            foreach (var col in cols)
            {
                double cLeft = col[0], cRight = col[1];
                double top = rTop + (rBot - rTop) * 0.25;
                double h = (rBot - rTop) * 0.5;
                double w = (cRight - cLeft) * 0.42;
                AddBlock(cLeft + (cRight - cLeft) * 0.06, top, w, h);
                AddBlock(cLeft + (cRight - cLeft) * 0.52, top, w, h);
            }
        }
    }

    private void AddBlock(double left, double top, double w, double h)
    {
        var block = new BoxView { Color = MapColor("#F7F8FA"), CornerRadius = 2 };
        AbsoluteLayout.SetLayoutBounds(block, new Rect(left, top, w, h));
        AbsoluteLayout.SetLayoutFlags(block, AbsoluteLayoutFlags.All);
        _mapSurface!.Children.Add(block);
    }

    private View BuildLegend()
    {
        var legend = new VerticalStackLayout { Spacing = 6 };
        var row = new HorizontalStackLayout { Spacing = 12 };
        row.Children.Add(LegendItem(MapColor("#F26A4B"), "🐶", "Perro"));
        row.Children.Add(LegendItem(MapColor("#E8A33D"), "🐱", "Gato"));
        legend.Children.Add(row);
        legend.Children.Add(new Label { Text = "Ubicaciones aproximadas", Style = Res("MutedText"), FontSize = 11 });
        return legend;
    }

    private View LegendItem(Color color, string emoji, string text)
    {
        var h = new HorizontalStackLayout { Spacing = 5 };
        var dot = new Border
        {
            WidthRequest = 14,
            HeightRequest = 14,
            Padding = 0,
            BackgroundColor = color,
            StrokeShape = new RoundRectangle { CornerRadius = 7 }
        };
        var em = new Label { Text = emoji, FontSize = 12 };
        var txt = new Label { Text = text, FontSize = 12, TextColor = MapColor("#172027"), VerticalTextAlignment = TextAlignment.Center };
        h.Children.Add(dot);
        h.Children.Add(em);
        h.Children.Add(txt);
        return h;
    }

    private void RebuildPins()
    {
        if (_mapSurface is null || _catalog is null) return;

        // Remove everything after the backdrop (pins + info panel)
        for (int i = _mapSurface.Children.Count - 1; i >= _backdropCount; i--)
            _mapSurface.Children.RemoveAt(i);
        _pinDots.Clear();
        _pinNames.Clear();

        foreach (var p in _catalog.Pets)
        {
            if (!MatchesFilter(p)) continue;

            if (p.Latitude < MockData.LAT_MIN || p.Latitude > MockData.LAT_MAX ||
                p.Longitude < MockData.LON_MIN || p.Longitude > MockData.LON_MAX)
            {
                continue;
            }

            double left = Math.Clamp(LonToLeft(p.Longitude), 2, 98);
            double top = Math.Clamp(LatToTop(p.Latitude), 2, 98);

            var pin = BuildPin(p);
            AbsoluteLayout.SetLayoutBounds(pin, new Rect(left / 100, top / 100, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
            AbsoluteLayout.SetLayoutFlags(pin, AbsoluteLayoutFlags.PositionProportional);
            _mapSurface.Children.Add(pin);
        }

        if (_infoPanel is not null)
        {
            AbsoluteLayout.SetLayoutBounds(_infoPanel, new Rect(1.0, 1.0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
            AbsoluteLayout.SetLayoutFlags(_infoPanel, AbsoluteLayoutFlags.PositionProportional);
            _mapSurface.Children.Add(_infoPanel);
        }

        UpdatePinStates();
    }

    private View BuildPin(Pet p)
    {
        var pin = new VerticalStackLayout { Spacing = 2 };

        var dot = new Border
        {
            WidthRequest = 18,
            HeightRequest = 18,
            Padding = 0,
            BackgroundColor = SpeciesColor(p),
            Stroke = Colors.White,
            StrokeThickness = 3,
            StrokeShape = new RoundRectangle { CornerRadius = 9 }
        };
        dot.Content = new Label
        {
            Text = SpeciesEmoji(p),
            FontSize = 10,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };
        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) => SelectPet(p);
        dot.GestureRecognizers.Add(tap);
        pin.Children.Add(dot);

        var name = new Label
        {
            Text = p.Name,
            FontSize = 10,
            TextColor = MapColor("#172027"),
            HorizontalTextAlignment = TextAlignment.Center
        };
        pin.Children.Add(name);

        _pinDots[p.Id] = dot;
        _pinNames[p.Id] = name;
        return pin;
    }

    private void UpdatePinStates()
    {
        if (_catalog is null) return;
        foreach (var kv in _pinDots)
        {
            var pet = _catalog.Pets.FirstOrDefault(x => x.Id == kv.Key);
            if (pet is null) continue;

            bool sel = _selectedPet is not null && _selectedPet.Id == kv.Key;
            var dot = kv.Value;
            dot.WidthRequest = sel ? 26 : 18;
            dot.HeightRequest = sel ? 26 : 18;
            dot.StrokeShape = new RoundRectangle { CornerRadius = sel ? 13 : 9 };
            dot.BackgroundColor = sel ? MapColor("#D7532D") : SpeciesColor(pet);
            dot.Opacity = sel ? 1.0 : 0.85;
            if (_pinNames.TryGetValue(kv.Key, out var name))
            {
                name.FontAttributes = sel ? FontAttributes.Bold : FontAttributes.None;
                name.Opacity = sel ? 1.0 : 0.7;
            }
        }
    }

    private void RebuildList()
    {
        if (_listContainer is null || _catalog is null) return;
        _listContainer.Children.Clear();
        _rows.Clear();

        foreach (var p in _catalog.Pets)
        {
            if (!MatchesFilter(p)) continue;

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                },
                ColumnSpacing = 8
            };
            var info = new VerticalStackLayout { Spacing = 2 };
            info.Children.Add(new Label { Text = p.Name, FontAttributes = FontAttributes.Bold });
            info.Children.Add(new Label { Text = p.Location, FontSize = 12, Style = Res("MutedText") });
            grid.Children.Add(info);

            var btn = new Button { Text = "Mostrar", Style = Res("SecondaryButton") };
            btn.Clicked += (s, e) => SelectPet(p);
            grid.Children.Add(btn);
            Grid.SetColumn(btn, 1);

            var card = new Border { Content = grid, BackgroundColor = Colors.White };
            var rowTap = new TapGestureRecognizer();
            rowTap.Tapped += (s, e) => SelectPet(p);
            card.GestureRecognizers.Add(rowTap);
            _rows[p.Id] = card;
            _listContainer.Children.Add(card);
        }
    }

    private void SelectPet(Pet pet)
    {
        _selectedPet = pet;
        UpdatePinStates();
        RenderInfo();
        HighlightList();
        _ = ScrollToRowAsync(pet);
    }

    private async Task ScrollToRowAsync(Pet pet)
    {
        if (_pageScroll is not null && _rows.TryGetValue(pet.Id, out var row))
        {
            try
            {
                await _pageScroll.ScrollToAsync(row, ScrollToPosition.MakeVisible, true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Scroll failed: {ex.Message}");
            }
        }
    }

    private void HighlightList()
    {
        foreach (var kv in _rows)
        {
            bool sel = _selectedPet is not null && _selectedPet.Id == kv.Key;
            kv.Value.BackgroundColor = sel ? MapColor("#FDEBDD") : Colors.White;
        }
    }

    private void RenderInfo()
    {
        if (_infoPanel is null || _infoContent is null) return;
        _infoContent.Children.Clear();

        if (_selectedPet is null)
        {
            _infoPanel.IsVisible = false;
            return;
        }

        _infoContent.Children.Add(new Label { Text = _selectedPet.Name, Style = Res("CardTitle") });
        _infoContent.Children.Add(new Label { Text = $"{_selectedPet.Species} - {_selectedPet.Breed}", Style = Res("MutedText") });
        _infoContent.Children.Add(new Label { Text = $"{_selectedPet.Location}", Style = Res("MutedText") });

        var detailBtn = new Button { Text = "Ver detalle" };
        detailBtn.Clicked += async (s, e) => await SafeGoToAsync($"petdetail?petId={_selectedPet.Id}");
        _infoContent.Children.Add(detailBtn);

        var closeBtn = new Button { Text = "Cerrar", Style = Res("SecondaryButton") };
        closeBtn.Clicked += (s, e) =>
        {
            _selectedPet = null;
            UpdatePinStates();
            HighlightList();
            RenderInfo();
        };
        _infoContent.Children.Add(closeBtn);

        _infoPanel.IsVisible = true;
    }

    private string SpeciesFilterValue()
    {
        var s = _speciesFilter?.SelectedItem?.ToString();
        if (s == "Perro" || s == "Gato") return s;
        return string.Empty;
    }

    private bool MatchesFilter(Pet p) => string.IsNullOrEmpty(_speciesFilterValue) || SpeciesKey(p) == _speciesFilterValue;

    private string SpeciesKey(Pet p) => IsCat(p) ? "Gato" : "Perro";

    private bool IsCat(Pet p) => p.Species.Contains("Gato", StringComparison.OrdinalIgnoreCase);

    private Color SpeciesColor(Pet p) => IsCat(p) ? MapColor("#E8A33D") : MapColor("#F26A4B");

    private string SpeciesEmoji(Pet p) => IsCat(p) ? "🐱" : "🐶";

    // Project (lat, lon) -> percentage of the surface, using the SAME Bogotá box
    // as the Web map so pins land in equivalent positions.
    private static double LatToTop(double lat) =>
        (MockData.LAT_MAX - lat) / (MockData.LAT_MAX - MockData.LAT_MIN) * 100;
    private static double LonToLeft(double lon) =>
        (lon - MockData.LON_MIN) / (MockData.LON_MAX - MockData.LON_MIN) * 100;

    private static Color MapColor(string hex) => Color.FromArgb(hex);

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
