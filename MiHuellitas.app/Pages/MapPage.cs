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

    // Street grid (keep in sync with MapView.razor)
    private static readonly double[] HStreets = { 0.16, 0.28, 0.44, 0.56, 0.68, 0.84 };
    private static readonly double[] VStreets = { 0.24, 0.32, 0.48, 0.58, 0.72, 0.88 };
    private static readonly string[] HNames = { "Calle 100", "Calle 72", "Calle 63", "Calle 45", "Calle 26", "Calle 13" };
    private static readonly string[] VNames = { "Av. Caracas", "NQS", "Cra 7", "Av. Boyacá", "Cra 13", "Av. 68" };

    // Landmark rectangles [top, left, height, width] (keep in sync with MapView.razor)
    private static readonly double[] ParkSB        = { 0.05, 0.08, 0.18, 0.16 };
    private static readonly double[] Airport        = { 0.32, 0.08, 0.14, 0.14 };
    private static readonly double[] Cerros         = { 0.00, 0.92, 1.00, 0.08 };
    private static readonly double[] LaCandelaria   = { 0.52, 0.36, 0.10, 0.12 };
    private static readonly double[] ParkTunal      = { 0.78, 0.30, 0.09, 0.12 };

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

        // ── Río Bogotá (west edge) ──
        AddZone(0.00, 0.04, 1.00, 0.035, "#DDEEFFB0", 6);
        PlaceLabel("Río Bogotá", 0.48, 0.03, 7, "#7FB3D3", true);

        // ── Parque Simón Bolívar (north-west) ──
        AddZone(ParkSB[0], ParkSB[1], ParkSB[2], ParkSB[3], "#CDE5CF", 10);
        PlaceLabel("Parque Simón Bolívar", 0.10, 0.11, 8, "#5A7A60", false, FontAttributes.Bold);

        // ── Aeropuerto El Dorado (west center) ──
        AddZone(Airport[0], Airport[1], Airport[2], Airport[3], "#E2ECF1", 10);
        // Runway
        var runway = new BoxView { Color = Colors.White.WithAlpha(0.65f), CornerRadius = 2 };
        AbsoluteLayout.SetLayoutBounds(runway, new Rect(0.10, 0.38, 0.10, 0.03));
        AbsoluteLayout.SetLayoutFlags(runway, AbsoluteLayoutFlags.All);
        _mapSurface.Children.Add(runway);
        PlaceLabel("Aeropuerto El Dorado", 0.34, 0.10, 7, "#6A8A9A", false, FontAttributes.Bold);

        // ── Parque El Tunal (south center) ──
        AddZone(ParkTunal[0], ParkTunal[1], ParkTunal[2], ParkTunal[3], "#D8EED9", 8);
        PlaceLabel("Parque El Tunal", 0.81, 0.32, 8, "#5A7A60", false, FontAttributes.Italic);

        // ── Plaza de Bolívar / La Candelaria (center-south) ──
        AddZone(LaCandelaria[0], LaCandelaria[1], LaCandelaria[2], LaCandelaria[3], "#FDF1E7", 8);
        PlaceLabel("La Candelaria", 0.54, 0.37, 8, "#B98A63", false, FontAttributes.Bold);
        PlaceLabel("Plaza de Bolívar", 0.57, 0.37, 7, "#B98A63", false, FontAttributes.Italic);

        // ── Cerros Orientales (east edge) ──
        AddZone(Cerros[0], Cerros[1], Cerros[2], Cerros[3], "#CDE5CF", 10);
        // Monserrate peak
        PlaceLabel("⛰️", 0.14, 0.93, 10, "#5A7A60", false);
        PlaceLabel("Monserrate", 0.135, 0.93, 7, "#5A7A60", false, FontAttributes.Italic);
        // Guadalupe peak
        PlaceLabel("⛰️", 0.36, 0.93, 10, "#5A7A60", false);
        PlaceLabel("Guadalupe", 0.355, 0.93, 7, "#5A7A60", false, FontAttributes.Italic);

        // ── City blocks ──
        PlaceCityBlocks();

        // ── Horizontal streets ──
        for (int i = 0; i < HStreets.Length; i++)
        {
            var road = new BoxView { Color = Colors.White };
            AbsoluteLayout.SetLayoutBounds(road, new Rect(0, HStreets[i], 0.92, 0.018));
            AbsoluteLayout.SetLayoutFlags(road, AbsoluteLayoutFlags.All);
            _mapSurface.Children.Add(road);

            var label = new Label { Text = HNames[i], FontSize = 9, TextColor = MapColor("#9AA6AD"), FontAttributes = FontAttributes.Italic };
            AbsoluteLayout.SetLayoutBounds(label, new Rect(10, HStreets[i], AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
            AbsoluteLayout.SetLayoutFlags(label, AbsoluteLayoutFlags.YProportional);
            _mapSurface.Children.Add(label);
        }

        // ── Vertical avenues ──
        for (int i = 0; i < VStreets.Length; i++)
        {
            var road = new BoxView { Color = Colors.White };
            AbsoluteLayout.SetLayoutBounds(road, new Rect(VStreets[i], 0, 0.016, 1));
            AbsoluteLayout.SetLayoutFlags(road, AbsoluteLayoutFlags.All);
            _mapSurface.Children.Add(road);

            var label = new Label { Text = VNames[i], FontSize = 9, TextColor = MapColor("#9AA6AD"), FontAttributes = FontAttributes.Italic };
            AbsoluteLayout.SetLayoutBounds(label, new Rect(VStreets[i], 8, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
            AbsoluteLayout.SetLayoutFlags(label, AbsoluteLayoutFlags.XProportional);
            _mapSurface.Children.Add(label);
        }

        // ── Diagonal streets ──
        AddDiagonal(30, 0.30, "Diag. 50");
        AddDiagonal(-30, 0.62, "Av. Jiménez");
    }

    private void PlaceCityBlocks()
    {
        if (_mapSurface is null) return;

        double[] hBorders = { 0.00, 0.16, 0.28, 0.44, 0.56, 0.68, 0.84, 0.92 };
        double[] vBorders = { 0.00, 0.24, 0.32, 0.48, 0.58, 0.72, 0.88, 0.92 };

        for (int r = 0; r < hBorders.Length - 1; r++)
        {
            for (int c = 0; c < vBorders.Length - 1; c++)
            {
                double bTop = hBorders[r], bBot = hBorders[r + 1];
                double bLeft = vBorders[c], bRight = vBorders[c + 1];
                if (Overlap(bTop, bBot, bLeft, bRight, ParkSB)) continue;
                if (Overlap(bTop, bBot, bLeft, bRight, Airport)) continue;
                if (Overlap(bTop, bBot, bLeft, bRight, Cerros)) continue;
                if (Overlap(bTop, bBot, bLeft, bRight, LaCandelaria)) continue;
                if (Overlap(bTop, bBot, bLeft, bRight, ParkTunal)) continue;
                double inset = 0.01;
                double w = (bRight - bLeft) - inset * 2;
                double h = (bBot - bTop) - inset * 2;
                if (w > 0.01 && h > 0.01)
                    AddBlock(bLeft + inset, bTop + inset, w, h);
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

    private void AddZone(double top, double left, double height, double width, string hex, double radius)
    {
        if (_mapSurface is null) return;
        var box = new BoxView { Color = MapColor(hex), CornerRadius = (float)radius };
        AbsoluteLayout.SetLayoutBounds(box, new Rect(left, top, width, height));
        AbsoluteLayout.SetLayoutFlags(box, AbsoluteLayoutFlags.All);
        _mapSurface.Children.Add(box);
    }

    private void PlaceLabel(string text, double top, double left, double fontSize, string color, bool isVertical, FontAttributes attrs = FontAttributes.None)
    {
        if (_mapSurface is null) return;
        var label = new Label
        {
            Text = text,
            FontSize = fontSize,
            TextColor = MapColor(color),
            FontAttributes = attrs,
            Rotation = isVertical ? -90 : 0
        };
        AbsoluteLayout.SetLayoutBounds(label, new Rect(left, top, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
        AbsoluteLayout.SetLayoutFlags(label, AbsoluteLayoutFlags.All);
        _mapSurface.Children.Add(label);
    }

    private static bool Overlap(double t, double b, double l, double r, double[] zone) =>
        t < zone[0] + zone[2] && b > zone[0] && l < zone[1] + zone[3] && r > zone[1];

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
        _infoContent.Children.Add(new Label { Text = $"{_selectedPet.Species} · {_selectedPet.Breed}", Style = Res("MutedText") });
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
