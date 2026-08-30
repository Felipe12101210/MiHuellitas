using Microsoft.Maui.Controls;
using MiHuellitas.shared.Services;
using System.Linq;

namespace MiHuellitas.app.Pages;

public class MapPage : ContentPage
{
    public MapPage()
    {
        Title = "Mapa";
        var catalog = MauiProgram.Services?.GetService(typeof(MockCatalogService)) as MockCatalogService;
        var layout = new VerticalStackLayout { Padding = 12, Spacing = 10 };

        layout.Children.Add(new Label { Text = "Mapa mock", FontAttributes = FontAttributes.Bold, FontSize = 18 });
        layout.Children.Add(new Label { Text = "El mapa es una representación mock. Selecciona una mascota en la lista.", FontSize = 12 });

        if (catalog is not null)
        {
            var list = new VerticalStackLayout { Spacing = 8 };
            foreach (var p in catalog.Pets)
            {
                var btn = new Button { Text = p.Name };
                btn.Clicked += async (s, e) => await Shell.Current.DisplayAlertAsync("Mascota seleccionada", $"{p.Name} - {p.Location}", "OK");
                list.Children.Add(btn);
            }
            layout.Children.Add(new ScrollView { Content = list, HeightRequest = 300 });
        }

        Content = new ScrollView { Content = layout };
    }
}
