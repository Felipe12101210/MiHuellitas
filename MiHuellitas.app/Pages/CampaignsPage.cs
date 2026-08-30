using Microsoft.Maui.Controls;
using MiHuellitas.shared.Services;
using System.Linq;

namespace MiHuellitas.app.Pages;

public class CampaignsPage : ContentPage
{
    private MockCatalogService? _catalog;
    private VerticalStackLayout? _itemsContainer;
    private string _searchTitle = string.Empty;
    private string _searchLocation = string.Empty;
    private int _pageSize = 6;

    public CampaignsPage()
    {
        Title = "Campañas";
        _catalog = MauiProgram.Services?.GetService(typeof(MockCatalogService)) as MockCatalogService;

        // Loading state is intentionally omitted: the mock catalog is a synchronous,
        // in-memory service, so there is no async fetch to wait for.
        var layout = new VerticalStackLayout { Padding = new Thickness(14, 18, 14, 0), Spacing = 12 };

        if (_catalog is null)
        {
            layout.Children.Add(new Label { Text = "Ocurrió un error al cargar las campañas. Intenta más tarde." });
        }
        else
        {
            layout.Children.Add(new Label { Text = "Eventos", Style = Res("Eyebrow") });
            layout.Children.Add(new Label { Text = "Campañas", Style = Res("PageTitle") });

            var filters = new VerticalStackLayout { Spacing = 8 };

            filters.Children.Add(new Label { Text = "Buscar por título", Style = Res("MutedText") });
            var titleEntry = new Entry { Placeholder = "Buscar por título" };
            titleEntry.TextChanged += (s, e) => { _searchTitle = e.NewTextValue ?? string.Empty; _pageSize = 6; BuildList(); };
            filters.Children.Add(titleEntry);

            filters.Children.Add(new Label { Text = "Buscar por ubicación", Style = Res("MutedText") });
            var locationEntry = new Entry { Placeholder = "Buscar por ubicación" };
            locationEntry.TextChanged += (s, e) => { _searchLocation = e.NewTextValue ?? string.Empty; _pageSize = 6; BuildList(); };
            filters.Children.Add(locationEntry);

            var clearBtn = new Button { Text = "Limpiar", Style = Res("SecondaryButton") };
            clearBtn.Clicked += (s, e) =>
            {
                _searchTitle = string.Empty;
                _searchLocation = string.Empty;
                titleEntry.Text = string.Empty;
                locationEntry.Text = string.Empty;
                _pageSize = 6;
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

        var filtered = _catalog.Campaigns
            .Where(c => string.IsNullOrEmpty(_searchTitle) || c.Title.Contains(_searchTitle, StringComparison.OrdinalIgnoreCase))
            .Where(c => string.IsNullOrEmpty(_searchLocation) || c.Location.Contains(_searchLocation, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!filtered.Any())
        {
            _itemsContainer.Children.Add(new Label { Text = "No se encontraron campañas que coincidan con los filtros.", HorizontalOptions = LayoutOptions.Center });
            return;
        }

        foreach (var c in filtered.Take(_pageSize))
        {
            var border = new Border { Padding = 10 };
            var v = new VerticalStackLayout { Spacing = 6 };
            if (!string.IsNullOrEmpty(c.ImageUrl))
                v.Children.Add(new Image { Source = c.ImageUrl, HeightRequest = 120, Aspect = Aspect.AspectFill });
            v.Children.Add(new Label { Text = c.Title, Style = Res("CardTitle") });
            v.Children.Add(new Label { Text = $"{c.Location} • {c.EventDate:dd MMM yyyy}", Style = Res("MutedText") });
            if (!string.IsNullOrEmpty(c.Description))
                v.Children.Add(new Label { Text = c.Description, Style = Res("MutedText") });
            v.Children.Add(new Label { Text = c.Organizer, Style = Res("MutedText") });
            v.Children.Add(new Label { Text = c.Status, Style = Res("BadgeTextAccent") });
            var btn = new Button { Text = "Ver detalle", Style = Res("SecondaryButton") };
            btn.Clicked += async (s, e) => await SafeGoToAsync($"campaigndetail?campaignId={c.Id}");
            v.Children.Add(btn);
            border.Content = v;
            _itemsContainer.Children.Add(border);
        }

        if (filtered.Count() > _pageSize)
        {
            var moreBtn = new Button { Text = "Cargar más", Style = Res("SecondaryButton") };
            moreBtn.Clicked += (s, e) => { _pageSize += 6; BuildList(); };
            _itemsContainer.Children.Add(moreBtn);
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
