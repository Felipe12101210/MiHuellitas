using Microsoft.Maui.Controls;
using MiHuellitas.shared.Services;
using System.Linq;

namespace MiHuellitas.app.Pages;

public class ProfilePage : ContentPage
{
    private MockCatalogService? _catalog;
    private Label _statReports = new Label();
    private Label _statPublications = new Label();
    private Label _statFavorites = new Label();
    private VerticalStackLayout _feedLayout = new VerticalStackLayout { Spacing = 8 };

    public ProfilePage()
    {
        Title = "Perfil";
        _catalog = MauiProgram.Services?.GetService(typeof(MockCatalogService)) as MockCatalogService;

        var layout = new VerticalStackLayout { Padding = 12, Spacing = 10 };

        if (_catalog is null)
        {
            layout.Children.Add(new Label { Text = "Servicio no disponible" });
            Content = layout;
            return;
        }

        var u = _catalog.CurrentUser;

        layout.Children.Add(new Label { Text = "Cuenta", Style = Res("Eyebrow") });
        layout.Children.Add(new Label { Text = "Perfil", Style = Res("PageTitle") });

        layout.Children.Add(new Image
        {
            Source = u.AvatarUrl,
            HeightRequest = 96,
            WidthRequest = 96,
            Aspect = Aspect.AspectFill,
            HorizontalOptions = LayoutOptions.Center
        });

        layout.Children.Add(new Label { Text = u.Name, FontAttributes = FontAttributes.Bold, FontSize = 18, HorizontalOptions = LayoutOptions.Center });
        layout.Children.Add(new Label { Text = u.City, HorizontalOptions = LayoutOptions.Center });
        layout.Children.Add(new Label { Text = u.Bio, HorizontalTextAlignment = TextAlignment.Center });

        var contact = new VerticalStackLayout { Spacing = 4 };
        contact.Children.Add(new Label { Text = u.Email, FontSize = 12 });
        contact.Children.Add(new Label { Text = u.Phone, FontSize = 12 });
        layout.Children.Add(contact);

        layout.Children.Add(BuildStatsGrid());

        layout.Children.Add(new Label { Text = "Actividad reciente", Style = Res("SectionTitle") });

        layout.Children.Add(_feedLayout);

        BuildFeed();

        Content = new ScrollView { Content = layout };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Ensure subscription (Shell may reuse the page instance). Unsubscribe first to avoid duplicates.
        if (_catalog is not null)
            _catalog.NotificationsChanged -= OnNotificationsChanged;
        if (_catalog is not null)
            _catalog.NotificationsChanged += OnNotificationsChanged;
        BuildFeed();
    }

    private Grid BuildStatsGrid()
    {
        var grid = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            }
        };

        grid.Children.Add(MakeStatBox("Reportes", _statReports, 0));
        grid.Children.Add(MakeStatBox("Publicaciones", _statPublications, 1));
        grid.Children.Add(MakeStatBox("Favoritos", _statFavorites, 2));
        return grid;
    }

    private Border MakeStatBox(string caption, Label value, int column)
    {
        var box = new Border { Padding = 10 };
        var content = new VerticalStackLayout { Spacing = 2 };
        value.FontAttributes = FontAttributes.Bold;
        value.FontSize = 18;
        value.HorizontalOptions = LayoutOptions.Center;
        content.Children.Add(value);
        content.Children.Add(new Label { Text = caption, FontSize = 12, HorizontalOptions = LayoutOptions.Center });
        box.Content = content;
        Grid.SetColumn(box, column);
        return box;
    }

    private void BuildFeed()
    {
        _feedLayout.Children.Clear();
        if (_catalog is null) return;

        _statReports.Text = _catalog.Reports.Count.ToString();
        _statPublications.Text = _catalog.Pets.Count(p => p.ResponsibleName == _catalog.CurrentUser.Name).ToString();
        _statFavorites.Text = _catalog.FavoritesCount.ToString();

        var notifications = _catalog.Notifications;
        if (!notifications.Any())
        {
            _feedLayout.Children.Add(new Label { Text = "Sin actividad reciente.", HorizontalOptions = LayoutOptions.Center });
            return;
        }

        foreach (var n in notifications)
        {
            var border = new Border { Padding = 10 };
            var v = new VerticalStackLayout();
            v.Children.Add(new Label { Text = n.Title, FontAttributes = FontAttributes.Bold });
            v.Children.Add(new Label { Text = n.Message, Style = Res("MutedText") });
            border.Content = v;
            _feedLayout.Children.Add(border);
        }
    }

    private void OnNotificationsChanged()
    {
        MainThread.BeginInvokeOnMainThread(() => BuildFeed());
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (_catalog is not null)
            _catalog.NotificationsChanged -= OnNotificationsChanged;
    }

    private static Style Res(string key) => (Style)Application.Current!.Resources[key];
}
