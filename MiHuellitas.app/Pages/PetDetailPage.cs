using Microsoft.Maui.Controls;
using MiHuellitas.shared.Services;
using MiHuellitas.shared.Models;
using System;

namespace MiHuellitas.app.Pages;

[QueryProperty(nameof(PetId), "petId")]
public class PetDetailPage : ContentPage
{
    private MockCatalogService? _catalog;
    private int _petId;

    public string PetId
    {
        set
        {
            if (int.TryParse(value, out var id))
            {
                _petId = id;
                LoadPet();
            }
        }
    }

    public PetDetailPage()
    {
        Title = "Detalle"
;        _catalog = MauiProgram.Services?.GetService(typeof(MockCatalogService)) as MockCatalogService;
        Content = new VerticalStackLayout { Padding = 12, Children = { new Label { Text = "Cargando..." } } };
    }

    private void LoadPet()
    {
        if (_catalog is null)
        {
            Content = new Label { Text = "Servicio no disponible" };
            return;
        }

        var pet = _catalog.Pets.FirstOrDefault(p => p.Id == _petId);
        if (pet is null)
        {
            Content = new Label { Text = "Mascota no encontrada." };
            return;
        }

        var layout = new VerticalStackLayout { Padding = 12, Spacing = 8 };
        if (!string.IsNullOrEmpty(pet.ImageUrl))
            layout.Children.Add(new Image { Source = pet.ImageUrl, HeightRequest = 200, Aspect = Aspect.AspectFill });
        layout.Children.Add(new Label { Text = pet.Name, FontAttributes = FontAttributes.Bold, FontSize = 20 });
        layout.Children.Add(new Label { Text = $"{pet.Species} • {pet.Breed} • {pet.Size}" });
        layout.Children.Add(new Label { Text = $"Ubicación: {pet.Location}" });
        layout.Children.Add(new Label { Text = pet.Description });
        layout.Children.Add(new Label { Text = $"Estado: {pet.Status}" });
        if (pet.ListingType == MiHuellitas.shared.Enums.ListingType.Lost || pet.ListingType == MiHuellitas.shared.Enums.ListingType.Found)
            layout.Children.Add(new Label { Text = $"Última vez visto: {pet.LastSeenAt:g}" });
        layout.Children.Add(new Label { Text = $"Responsable: {pet.ResponsibleName}" });
        layout.Children.Add(new Label { Text = $"Tel: {pet.ContactPhone}" });

        var reportBtn = new Button { Text = "Reportar" };
        reportBtn.Clicked += async (s, e) =>
        {
            // navigate to a simple report form (reuse PublishPage flow would be different); open a prompt
            string res = await DisplayPromptAsync("Reportar", "Mensaje de reporte:");
            if (!string.IsNullOrWhiteSpace(res))
            {
                var report = new Report { PetId = pet.Id, ReporterName = _catalog.CurrentUser.Name, ReporterPhone = _catalog.CurrentUser.Phone, Type = "Reporte", Message = res };
                _catalog.SubmitReport(report);
                await DisplayAlertAsync("Gracias", "Reporte enviado (mock)", "OK");
            }
        };

        layout.Children.Add(reportBtn);

        Content = new ScrollView { Content = layout };
    }
}
