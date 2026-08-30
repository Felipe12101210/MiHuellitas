using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel.Communication;
using MiHuellitas.shared.Services;
using MiHuellitas.shared.Models;
using System;
using System.Linq;

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
        Title = "Detalle";
        _catalog = MauiProgram.Services?.GetService(typeof(MockCatalogService)) as MockCatalogService;
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
        layout.Children.Add(new Label { Text = "Mascota", Style = Res("Eyebrow") });
        if (!string.IsNullOrEmpty(pet.ImageUrl))
            layout.Children.Add(new Image { Source = pet.ImageUrl, HeightRequest = 200, Aspect = Aspect.AspectFill });
        layout.Children.Add(new Label { Text = pet.Name, Style = Res("PageTitle") });
        layout.Children.Add(new Label { Text = $"{pet.Species} • {pet.Breed} • {pet.Size}", Style = Res("MutedText") });
        layout.Children.Add(new Label { Text = $"Ubicación: {pet.Location}", Style = Res("MutedText") });
        layout.Children.Add(new Label { Text = pet.Description, Style = Res("MutedText") });
        layout.Children.Add(new Label { Text = pet.Status.ToString(), Style = Res("BadgeText") });
        if (pet.ListingType == MiHuellitas.shared.Enums.ListingType.Lost || pet.ListingType == MiHuellitas.shared.Enums.ListingType.Found)
            layout.Children.Add(new Label { Text = $"Última vez visto: {pet.LastSeenAt:g}", Style = Res("MutedText") });
        layout.Children.Add(new Label { Text = $"Responsable: {pet.ResponsibleName}", Style = Res("MutedText") });
        layout.Children.Add(new Label { Text = $"Tel: {pet.ContactPhone}", Style = Res("MutedText") });

        var callText = pet.ListingType == MiHuellitas.shared.Enums.ListingType.Adoption ? "Llamar" : "Contactar";
        var callBtn = new Button { Text = callText };
        callBtn.Clicked += async (s, e) => await OpenPhone(pet.ContactPhone);
        layout.Children.Add(callBtn);

        var favBtn = new Button { Text = _catalog.IsFavorite(pet.Id) ? "Quitar favorito" : "Favorito", Style = Res("SecondaryButton") };
        favBtn.Clicked += async (s, e) =>
        {
            _catalog!.ToggleFavorite(pet.Id);
            var isFav = _catalog.IsFavorite(pet.Id);
            favBtn.Text = isFav ? "Quitar favorito" : "Favorito";
            await DisplayAlertAsync("Favoritos", isFav ? "Mascota añadida a favoritos." : "Mascota eliminada de favoritos.", "OK");
        };
        layout.Children.Add(favBtn);

        var backBtn = new Button { Text = "Volver", Style = Res("SecondaryButton") };
        backBtn.Clicked += async (s, e) => await GoBackAsync();
        layout.Children.Add(backBtn);

        var reportHeader = pet.ListingType == MiHuellitas.shared.Enums.ListingType.Adoption
            ? "Enviar reporte / mostrar interés"
            : "Reportar información / contactar responsable";
        layout.Children.Add(new Label { Text = reportHeader, Style = Res("SectionTitle") });

        var reportName = new Entry { Placeholder = "Nombre" };
        var reportPhone = new Entry { Placeholder = "Teléfono", Keyboard = Keyboard.Telephone };
        var reportType = new Picker { Title = "Tipo" };
        reportType.Items.Add("Info");
        reportType.Items.Add("Sighted");
        reportType.Items.Add("Issue");
        reportType.SelectedIndex = 0;
        var reportMessage = new Editor { Placeholder = "Mensaje", AutoSize = EditorAutoSizeOption.TextChanges, MinimumHeightRequest = 80 };

        var reportFields = new VerticalStackLayout { Spacing = 8 };
        reportFields.Children.Add(new Label { Text = "Nombre", Style = Res("MutedText"), FontSize = 12 });
        reportFields.Children.Add(reportName);
        reportFields.Children.Add(new Label { Text = "Teléfono", Style = Res("MutedText"), FontSize = 12 });
        reportFields.Children.Add(reportPhone);
        reportFields.Children.Add(new Label { Text = "Tipo", Style = Res("MutedText"), FontSize = 12 });
        reportFields.Children.Add(reportType);
        reportFields.Children.Add(new Label { Text = "Mensaje", Style = Res("MutedText"), FontSize = 12 });
        reportFields.Children.Add(reportMessage);

        var sendReportBtn = new Button { Text = "Enviar reporte" };
        sendReportBtn.Clicked += async (s, e) =>
        {
            if (_catalog is null)
            {
                await DisplayAlertAsync("Error", "Servicio no disponible", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(reportName.Text))
            {
                await DisplayAlertAsync("Validación", "Por favor ingresa tu nombre.", "OK");
                return;
            }

            var digits = new string((reportPhone.Text ?? string.Empty).Where(char.IsDigit).ToArray());
            if (digits.Length < 7)
            {
                await DisplayAlertAsync("Validación", "Por favor ingresa un teléfono válido (mínimo 7 dígitos).", "OK");
                return;
            }

            var report = new Report
            {
                PetId = pet.Id,
                ReporterName = reportName.Text?.Trim() ?? string.Empty,
                ReporterPhone = reportPhone.Text?.Trim() ?? string.Empty,
                Type = reportType.SelectedItem?.ToString() ?? "Info",
                Message = reportMessage.Text ?? string.Empty
            };

            _catalog.SubmitReport(report);

            await DisplayAlertAsync("Gracias", "Gracias — hemos recibido tu reporte.", "OK");

            reportName.Text = string.Empty;
            reportPhone.Text = string.Empty;
            reportMessage.Text = string.Empty;
            reportType.SelectedIndex = 0;
        };

        layout.Children.Add(new Border { Padding = 12, Content = reportFields });
        layout.Children.Add(sendReportBtn);

        Content = new ScrollView { Content = layout };
    }

    private static Style Res(string key) => (Style)Application.Current!.Resources[key];

    private async Task OpenPhone(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            await DisplayAlertAsync("Contacto", "No hay teléfono de contacto disponible.", "OK");
            return;
        }

        try
        {
            if (PhoneDialer.IsSupported)
            {
                PhoneDialer.Open(number);
            }
            else
            {
                // El emulador o dispositivo no permite abrir el marcador; degrada con un aviso.
                await DisplayAlertAsync("Contacto", $"Llama al {number}", "OK");
            }
        }
        catch (Exception)
        {
            // Si el marcador falla, mostramos el número para que la persona llame manualmente.
            await DisplayAlertAsync("Contacto", $"Llama al {number}", "OK");
        }
    }

    private async Task SafeGoToAsync(string route)
    {
        try
        {
            await Shell.Current.GoToAsync(route);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation failed ({route}): {ex.Message}");
        }
    }

    private async Task GoBackAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GoBack failed: {ex.Message}");
            await Navigation.PopAsync();
        }
    }
}
