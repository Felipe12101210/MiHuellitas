using Microsoft.Maui.Controls;
using MiHuellitas.shared.Services;
using System.Linq;

namespace MiHuellitas.app.Pages;

public class LostPetsPage : ContentPage
{
    public LostPetsPage()
    {
        Title = "Perdidos";
        var catalog = MauiProgram.Services?.GetService(typeof(MockCatalogService)) as MockCatalogService;
        var layout = new VerticalStackLayout { Padding = 12, Spacing = 10 };

        if (catalog is null)
        {
            layout.Children.Add(new Label { Text = "Servicio no disponible" });
        }
        else
        {
            if (!catalog.LostPets.Any())
            {
                layout.Children.Add(new Label { Text = "No hay mascotas perdidas.", HorizontalOptions = LayoutOptions.Center });
            }
            else
            {
                var list = new VerticalStackLayout { Spacing = 8 };
                foreach (var p in catalog.LostPets)
                {
                    var frame = new Frame { Padding = 8, BorderColor = Colors.LightGray, CornerRadius = 8 };
                    var v = new VerticalStackLayout();
                    v.Children.Add(new Label { Text = p.Name, FontAttributes = FontAttributes.Bold });
                    v.Children.Add(new Label { Text = $"{p.LastSeenAt:g} • {p.Location}", FontSize = 12 });
                    frame.Content = v;
                    list.Children.Add(frame);
                }
                layout.Children.Add(new ScrollView { Content = list });
            }
        }

        Content = layout;
    }
}
