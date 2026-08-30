using Microsoft.Maui.Controls;
using MiHuellitas.shared.Services;
using MiHuellitas.shared.Models;
using System.Linq;
using System;

namespace MiHuellitas.app.Pages;

public class NotificationsPage : ContentPage
{
    private MockCatalogService? _catalog;
    private VerticalStackLayout _listLayout = new VerticalStackLayout { Spacing = 8 };

    public NotificationsPage()
    {
        Title = "Notificaciones";
        _catalog = MauiProgram.Services?.GetService(typeof(MockCatalogService)) as MockCatalogService;

        var layout = new VerticalStackLayout { Padding = new Thickness(14, 18, 14, 0), Spacing = 12 };
        layout.Children.Add(new Label { Text = "Centro", Style = Res("Eyebrow") });
        layout.Children.Add(new Label { Text = "Notificaciones", Style = Res("PageTitle") });

        if (_catalog is null)
        {
            layout.Children.Add(new Label { Text = "Servicio no disponible" });
            Content = layout;
            return;
        }

        BuildList();

        layout.Children.Add(_listLayout);
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
    }

    private void BuildList()
    {
        _listLayout.Children.Clear();
        if (_catalog is null) return;
        var notifications = _catalog.Notifications;
        if (!notifications.Any())
        {
            _listLayout.Children.Add(new Label { Text = "No hay notificaciones.", HorizontalOptions = LayoutOptions.Center });
            return;
        }

        foreach (var n in notifications)
        {
            var border = new Border
            {
                BackgroundColor = n.IsRead ? (Color)Application.Current!.Resources["SurfaceSoft"] : Color.FromArgb("#FFF7E6")
            };
            var v = new VerticalStackLayout { Spacing = 4 };

            var header = new Grid { ColumnSpacing = 8, ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) } };
            header.Children.Add(new Label { Text = n.Type, Style = Res("BadgeText") });
            var dateLabel = new Label { Text = n.CreatedAt.ToString("dd MMM yyyy, HH:mm"), Style = Res("MutedText"), HorizontalOptions = LayoutOptions.End };
            Grid.SetColumn(dateLabel, 1);
            header.Children.Add(dateLabel);
            v.Children.Add(header);

            v.Children.Add(new Label { Text = n.Title, FontAttributes = FontAttributes.Bold });
            v.Children.Add(new Label { Text = n.Message, FontSize = 12 });

            var actions = new HorizontalStackLayout { Spacing = 8 };
            if (!n.IsRead)
            {
                var markBtn = new Button { Text = "Marcar leída" };
                markBtn.Clicked += async (s, e) =>
                {
                    _catalog?.MarkNotificationRead(n.Id);
                    await DisplayAlertAsync("Notificaciones", "Notificación marcada como leída.", "OK");
                };
                actions.Children.Add(markBtn);
            }
            var delBtn = new Button { Text = "Eliminar", BackgroundColor = Color.FromArgb("#E74C3C") };
            delBtn.Clicked += (s, e) => { _catalog?.RemoveNotification(n.Id); };
            actions.Children.Add(delBtn);

            v.Children.Add(actions);
            border.Content = v;
            _listLayout.Children.Add(border);
        }
    }

    private void OnNotificationsChanged()
    {
        MainThread.BeginInvokeOnMainThread(() => BuildList());
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (_catalog is not null)
            _catalog.NotificationsChanged -= OnNotificationsChanged;
    }

    private static Style Res(string key) => (Style)Application.Current!.Resources[key];
}
