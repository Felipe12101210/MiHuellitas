using Microsoft.Maui.Controls;
using MiHuellitas.shared.Services;
using System.Linq;

namespace MiHuellitas.app.Pages;

public class AdoptionsPage : ContentPage
{
    private MockCatalogService? _catalog;
    private VerticalStackLayout? _itemsContainer;
    private Picker? _speciesPicker;
    private Picker? _breedPicker;
    private Picker? _locationPicker;
    private Picker? _statusPicker;
    private string _selectedSpecies = string.Empty;
    private string _selectedBreed = string.Empty;
    private string _selectedLocation = string.Empty;
    private string _selectedStatus = string.Empty;
    private string _searchText = string.Empty;
    private int _pageSize = 6;
    private Entry? _searchEntry;
    private bool _populatingOptions;

    public AdoptionsPage()
    {
        Title = "Adopciones";
        _catalog = MauiProgram.Services?.GetService(typeof(MockCatalogService)) as MockCatalogService;

        // Loading state is intentionally omitted: the mock catalog is a synchronous,
        // in-memory service, so there is no async fetch to wait for.
        var layout = new VerticalStackLayout { Padding = new Thickness(14, 18, 14, 0), Spacing = 12 };

        if (_catalog is null)
        {
            layout.Children.Add(new Label { Text = "Ocurrió un error al cargar las mascotas. Intenta más tarde." });
        }
        else
        {
            layout.Children.Add(new Label { Text = "Adopción", Style = Res("Eyebrow") });
            layout.Children.Add(new Label { Text = "Adopciones", Style = Res("PageTitle") });

            var filters = new VerticalStackLayout { Spacing = 8 };

            filters.Children.Add(new Label { Text = "Buscar", Style = Res("MutedText") });
            _searchEntry = new Entry { Placeholder = "Buscar por nombre, descripción o ubicación" };
            _searchEntry.TextChanged += (s, e) =>
            {
                _searchText = e.NewTextValue ?? string.Empty;
                _pageSize = 6;
                BuildList();
            };
            filters.Children.Add(_searchEntry);

            _speciesPicker = AddFilterRow(filters, "Especie", "Todas", (picker) => { _selectedSpecies = PickerSelection(picker, "Todas"); _pageSize = 6; BuildList(); });
            _breedPicker = AddFilterRow(filters, "Raza", "Todas", (picker) => { _selectedBreed = PickerSelection(picker, "Todas"); _pageSize = 6; BuildList(); });
            _locationPicker = AddFilterRow(filters, "Ubicación", "Todas", (picker) => { _selectedLocation = PickerSelection(picker, "Todas"); _pageSize = 6; BuildList(); });
            _statusPicker = AddFilterRow(filters, "Estado", "Todos", (picker) => { _selectedStatus = PickerSelection(picker, "Todos"); _pageSize = 6; BuildList(); });

            var clearBtn = new Button { Text = "Limpiar", Style = Res("SecondaryButton") };
            clearBtn.Clicked += (s, e) => ClearFilters();
            filters.Children.Add(clearBtn);

            BuildFilterOptions();

            _itemsContainer = new VerticalStackLayout { Spacing = 8 };
            var filtersPanel = new Border { Padding = 12, Content = filters };
            layout.Children.Add(filtersPanel);
            layout.Children.Add(_itemsContainer);

            BuildList();
        }

        Content = new ScrollView { Content = layout };
    }

    private Picker AddFilterRow(VerticalStackLayout parent, string label, string allLabel, System.Action<Picker> onChanged)
    {
        var row = new VerticalStackLayout { Spacing = 2 };
        row.Children.Add(new Label { Text = label, Style = Res("MutedText") });
        var picker = new Picker { Title = allLabel };
        picker.SelectedIndexChanged += (s, e) => { if (!_populatingOptions) onChanged(picker); };
        row.Children.Add(picker);
        parent.Children.Add(row);
        return picker;
    }

    private void BuildFilterOptions()
    {
        _populatingOptions = true;
        Populate(_speciesPicker, _catalog!.Adoptions.Select(p => p.Species), "Todas");
        Populate(_breedPicker, _catalog.Adoptions.Select(p => p.Breed), "Todas");
        Populate(_locationPicker, _catalog.Adoptions.Select(p => p.Location), "Todas");
        Populate(_statusPicker, _catalog.Adoptions.Select(p => p.Status.ToString()), "Todos");
        _populatingOptions = false;
    }

    private static void Populate(Picker? picker, IEnumerable<string> options, string allLabel)
    {
        if (picker is null) return;
        picker.Items.Clear();
        picker.Items.Add(allLabel);
        foreach (var o in options.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x))
            picker.Items.Add(o);
        picker.SelectedIndex = 0;
    }

    private static string PickerSelection(Picker? picker, string allLabel)
    {
        if (picker is null) return string.Empty;
        var sel = picker.SelectedItem?.ToString();
        return sel is null || sel == allLabel ? string.Empty : sel;
    }

    private void BuildList()
    {
        if (_itemsContainer is null || _catalog is null) return;
        _itemsContainer.Children.Clear();

        var filtered = _catalog.Adoptions
            .Where(p => string.IsNullOrEmpty(_selectedSpecies) || p.Species == _selectedSpecies)
            .Where(p => string.IsNullOrEmpty(_selectedBreed) || p.Breed == _selectedBreed)
            .Where(p => string.IsNullOrEmpty(_selectedLocation) || p.Location == _selectedLocation)
            .Where(p => string.IsNullOrEmpty(_selectedStatus) || p.Status.ToString() == _selectedStatus)
            .Where(p => string.IsNullOrEmpty(_searchText) ||
                (p.Name + " " + p.Description + " " + p.Location).Contains(_searchText, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!filtered.Any())
        {
            _itemsContainer.Children.Add(new Label { Text = "No se encontraron animales que coincidan con los filtros.", HorizontalOptions = LayoutOptions.Center });
            return;
        }

        foreach (var p in filtered.Take(_pageSize))
        {
            var border = new Border { Padding = 0 };
            var v = new VerticalStackLayout { Spacing = 8 };
            if (!string.IsNullOrEmpty(p.ImageUrl))
                v.Children.Add(new Image { Source = p.ImageUrl, HeightRequest = 150, Aspect = Aspect.AspectFill });
            var info = new VerticalStackLayout { Spacing = 6, Padding = new Thickness(12, 10, 12, 12) };
            info.Children.Add(new Label { Text = p.Name, Style = Res("CardTitle") });
            info.Children.Add(new Label { Text = $"{p.Species} • {p.Breed} • {p.Size}", Style = Res("MutedText") });
            info.Children.Add(new Label { Text = $"📍 {p.Location}", Style = Res("MutedText") });
            info.Children.Add(new Label { Text = p.Status.ToString(), Style = Res("BadgeText") });
            var tap = new TapGestureRecognizer();
            tap.Tapped += async (s, e) => await SafeGoToAsync($"petdetail?petId={p.Id}");
            v.GestureRecognizers.Add(tap);
            var btn = new Button { Text = "Ver detalle", Style = Res("SecondaryButton") };
            btn.Clicked += async (s, e) => await SafeGoToAsync($"petdetail?petId={p.Id}");
            info.Children.Add(btn);
            v.Children.Add(info);
            border.Content = v;
            _itemsContainer.Children.Add(border);
        }

        if (filtered.Count() > _pageSize)
        {
            var moreBtn = new Button { Text = "Cargar más" };
            moreBtn.Clicked += (s, e) => { _pageSize += 6; BuildList(); };
            _itemsContainer.Children.Add(moreBtn);
        }
    }

    private void ClearFilters()
    {
        _searchText = string.Empty;
        if (_searchEntry is not null) _searchEntry.Text = string.Empty;
        _selectedSpecies = string.Empty;
        _selectedBreed = string.Empty;
        _selectedLocation = string.Empty;
        _selectedStatus = string.Empty;
        _populatingOptions = true;
        if (_speciesPicker is not null) _speciesPicker.SelectedIndex = 0;
        if (_breedPicker is not null) _breedPicker.SelectedIndex = 0;
        if (_locationPicker is not null) _locationPicker.SelectedIndex = 0;
        if (_statusPicker is not null) _statusPicker.SelectedIndex = 0;
        _populatingOptions = false;
        _pageSize = 6;
        BuildList();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Ensure subscription (Shell may reuse the page instance). Unsubscribe first to avoid duplicates.
        if (_catalog is not null)
            _catalog.PetsChanged -= OnPetsChanged;
        if (_catalog is not null)
            _catalog.PetsChanged += OnPetsChanged;
    }

    private void OnPetsChanged()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_catalog is null) return;
            BuildFilterOptions();
            BuildList();
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (_catalog is not null) _catalog.PetsChanged -= OnPetsChanged;
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
