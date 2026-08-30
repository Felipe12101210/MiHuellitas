using Microsoft.Maui.Controls;
using MiHuellitas.shared.Services;
using System.Linq;

namespace MiHuellitas.app.Pages;

public class FoundPetsPage : ContentPage
{
    public FoundPetsPage()
    {
        Title = "Encontrados";
        var catalog = MauiProgram.Services?.GetService(typeof(MockCatalogService)) as MockCatalogService;
        var layout = new VerticalStackLayout { Padding = 12, Spacing = 10 };

        if (catalog is null)
        {
            layout.Children.Add(new Label { Text = "Servicio no disponible" });
        }
        else
        {
            if (!catalog.FoundPets.Any())
            {
                layout.Children.Add(new Label { Text = "No hay mascotas encontradas.", HorizontalOptions = LayoutOptions.Center });
            }
            else
            {
                var list = new VerticalStackLayout { Spacing = 8 };
                foreach (var p in catalog.FoundPets)
                {
                    var border = new Border { Padding = 8, Stroke = Colors.LightGray, StrokeThickness = 1, CornerRadius = new CornerRadius(8) };
                    var v = new VerticalStackLayout();
                    v.Children.Add(new Label { Text = p.Name, FontAttributes = FontAttributes.Bold });
                    v.Children.Add(new Label { Text = $"{p.LastSeenAt:g} • {p.Location}", FontSize = 12 });
                    var btn = new Button { Text = "Ver detalle" };
                    btn.Clicked += async (s, e) => await Shell.Current.GoToAsync($"petdetail?petId={p.Id}");
                    v.Children.Add(btn);
                    border.Content = v;
                    list.Children.Add(border);
                }
                layout.Children.Add(new ScrollView { Content = list });
            }
        }

        Content = layout;
    }
}
