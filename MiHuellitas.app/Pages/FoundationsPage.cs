using Microsoft.Maui.Controls;
using MiHuellitas.shared.Services;
using System.Linq;

namespace MiHuellitas.app.Pages;

public class FoundationsPage : ContentPage
{
    public FoundationsPage()
    {
        Title = "Fundaciones";
        var catalog = MauiProgram.Services?.GetService(typeof(MockCatalogService)) as MockCatalogService;
        var layout = new VerticalStackLayout { Padding = 12, Spacing = 10 };

        if (catalog is null)
        {
            layout.Children.Add(new Label { Text = "Servicio no disponible" });
        }
        else
        {
            if (!catalog.Foundations.Any())
            {
                layout.Children.Add(new Label { Text = "No hay fundaciones.", HorizontalOptions = LayoutOptions.Center });
            }
            else
            {
                var list = new VerticalStackLayout { Spacing = 8 };
                foreach (var f in catalog.Foundations)
                {
                    var border = new Border { Padding = 8, Stroke = Colors.LightGray, StrokeThickness = 1 };
                    var v = new VerticalStackLayout();
                    v.Children.Add(new Label { Text = f.Name, FontAttributes = FontAttributes.Bold });
                    v.Children.Add(new Label { Text = $"{f.Location}", FontSize = 12 });
                    border.Content = v;
                    list.Children.Add(border);
                }
                layout.Children.Add(new ScrollView { Content = list });
            }
        }

        Content = layout;
    }
}
