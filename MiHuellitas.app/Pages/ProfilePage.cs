using Microsoft.Maui.Controls;
using MiHuellitas.shared.Services;
using System.Linq;

namespace MiHuellitas.app.Pages;

public class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        Title = "Perfil";
        var catalog = MauiProgram.Services?.GetService(typeof(MockCatalogService)) as MockCatalogService;
        var layout = new VerticalStackLayout { Padding = 12, Spacing = 10 };

        if (catalog is null)
        {
            layout.Children.Add(new Label { Text = "Servicio no disponible" });
        }
        else
        {
            var u = catalog.CurrentUser;
            layout.Children.Add(new Label { Text = u.Name, FontAttributes = FontAttributes.Bold, FontSize = 18 });
            layout.Children.Add(new Label { Text = u.Email });
            layout.Children.Add(new Label { Text = u.Phone });
            layout.Children.Add(new Label { Text = u.City });
            layout.Children.Add(new Label { Text = u.Bio });

            layout.Children.Add(new Label { Text = $"Publicaciones: {catalog.Pets.Count(p => p.ResponsibleName == u.Name)}" });
            layout.Children.Add(new Label { Text = $"Reportes: {catalog.Reports.Count}" });
            layout.Children.Add(new Label { Text = $"Notificaciones: {catalog.Notifications.Count}" });
        }

        Content = new ScrollView { Content = layout };
    }
}
