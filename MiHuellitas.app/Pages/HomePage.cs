using Microsoft.Maui.Controls;
using MiHuellitas.shared.Services;

namespace MiHuellitas.app.Pages;

public class HomePage : ContentPage
{
    public HomePage()
    {
        Title = "MiHuellitas";
        var catalog = MauiProgram.Services?.GetService(typeof(MockCatalogService)) as MockCatalogService;

        var layout = new VerticalStackLayout { Padding = 12, Spacing = 10 };
        layout.Children.Add(new Label { Text = "Bienvenido a MiHuellitas", FontAttributes = FontAttributes.Bold, FontSize = 20 });

        if (catalog is null)
        {
            layout.Children.Add(new Label { Text = "Servicio no disponible" });
        }
        else
        {
            layout.Children.Add(new Button { Text = "Adopciones", Command = new Command(async () => await Shell.Current.GoToAsync("/adoptions")) });
            layout.Children.Add(new Button { Text = "Perdidos", Command = new Command(async () => await Shell.Current.GoToAsync("/lost")) });
            layout.Children.Add(new Button { Text = "Encontrados", Command = new Command(async () => await Shell.Current.GoToAsync("/found")) });
            layout.Children.Add(new Button { Text = "Fundaciones", Command = new Command(async () => await Shell.Current.GoToAsync("/foundations")) });
            layout.Children.Add(new Button { Text = "Campañas", Command = new Command(async () => await Shell.Current.GoToAsync("/campaigns")) });
            layout.Children.Add(new Button { Text = "Mapa", Command = new Command(async () => await Shell.Current.GoToAsync("/map")) });
            layout.Children.Add(new Button { Text = "Notificaciones", Command = new Command(async () => await Shell.Current.GoToAsync("/notifications")) });
            layout.Children.Add(new Button { Text = "Perfil", Command = new Command(async () => await Shell.Current.GoToAsync("/profile")) });
            layout.Children.Add(new Button { Text = "Publicar mascota", Command = new Command(async () => await Shell.Current.GoToAsync("/publish")) });
        }

        Content = new ScrollView { Content = layout };
    }
}
