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

        var layout = new VerticalStackLayout { Padding = 12, Spacing = 10 };
        layout.Children.Add(new Label { Text = "Notificaciones", FontAttributes = FontAttributes.Bold, FontSize = 18 });

        if (_catalog is null)
        {
            layout.Children.Add(new Label { Text = "Servicio no disponible" });
            Content = layout;
            return;
        }

        BuildList();

        var scroll = new ScrollView { Content = _listLayout };
        layout.Children.Add(scroll);
        Content = layout;

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
            var border = new Border { Padding = 8, Stroke = Colors.LightGray, StrokeThickness = 1 };
            var v = new VerticalStackLayout();
            v.Children.Add(new Label { Text = n.Title, FontAttributes = FontAttributes.Bold });
            v.Children.Add(new Label { Text = n.Message, FontSize = 12 });
            var actions = new HorizontalStackLayout { Spacing = 8 };
            var markBtn = new Button { Text = n.IsRead ? "Leída" : "Marcar leída" };
            markBtn.Clicked += (s, e) => { _catalog?.MarkNotificationRead(n.Id); };
            var delBtn = new Button { Text = "Eliminar", BackgroundColor = Colors.LightPink };
            delBtn.Clicked += (s, e) => { _catalog?.RemoveNotification(n.Id); };
            actions.Children.Add(markBtn);
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
}
