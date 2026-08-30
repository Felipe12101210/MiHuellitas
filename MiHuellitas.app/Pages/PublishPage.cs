using Microsoft.Maui.Controls;
using MiHuellitas.shared.Services;
using MiHuellitas.shared.Models;
using MiHuellitas.shared.Enums;
using System;
using System.Linq;

namespace MiHuellitas.app.Pages;

public class PublishPage : ContentPage
{
    private Entry _nameEntry;
    private Entry _speciesEntry;
    private Entry _breedEntry;
    private Entry _ageEntry;
    private Entry _sexEntry;
    private Entry _sizeEntry;
    private Entry _locationEntry;
    private Picker _typePicker;
    private Picker _statusPicker;
    private Entry _responsibleEntry;
    private Entry _phoneEntry;
    private Picker _foundationPicker;
    private Editor _descriptionEntry;
    private MockCatalogService? _catalog;

    public PublishPage()
    {
        Title = "Publicar";
        _catalog = MauiProgram.Services?.GetService(typeof(MockCatalogService)) as MockCatalogService;

        var layout = new VerticalStackLayout { Padding = new Thickness(14, 18, 14, 0), Spacing = 12 };

        layout.Children.Add(new Label { Text = "Publicar", Style = Res("Eyebrow") });
        layout.Children.Add(new Label { Text = "Nueva publicación", Style = Res("PageTitle") });

        _nameEntry = new Entry { Placeholder = "Nombre *" };
        _speciesEntry = new Entry { Placeholder = "Especie *" };
        _breedEntry = new Entry { Placeholder = "Raza" };
        _ageEntry = new Entry { Placeholder = "Edad (meses)", Keyboard = Keyboard.Numeric };
        _sexEntry = new Entry { Placeholder = "Sexo" };
        _sizeEntry = new Entry { Placeholder = "Tamaño" };
        _locationEntry = new Entry { Placeholder = "Ubicación" };

        _typePicker = new Picker { Title = "Tipo de anuncio *" };
        foreach (var lt in Enum.GetValues<ListingType>())
            _typePicker.Items.Add(lt.ToString());
        _typePicker.SelectedIndex = 0;

        _statusPicker = new Picker { Title = "Estado *" };
        foreach (var st in Enum.GetValues<PetStatus>())
            _statusPicker.Items.Add(st.ToString());
        _statusPicker.SelectedIndex = 0;

        _responsibleEntry = new Entry { Placeholder = "Responsable" };
        _phoneEntry = new Entry { Placeholder = "Teléfono de contacto", Keyboard = Keyboard.Telephone };

        _foundationPicker = new Picker { Title = "Fundación (opcional)" };
        _foundationPicker.Items.Add("Ninguna");
        if (_catalog is not null)
        {
            foreach (var f in _catalog.GetFoundations())
                _foundationPicker.Items.Add(f.Name);
        }

        _descriptionEntry = new Editor { Placeholder = "Descripción", MinimumHeightRequest = 100 };

        // Mascota
        layout.Children.Add(new Label { Text = "Mascota", Style = Res("SectionTitle") });
        var petFields = new VerticalStackLayout { Spacing = 8 };
        AddField(petFields, "Nombre", _nameEntry);
        AddField(petFields, "Especie", _speciesEntry);
        AddField(petFields, "Raza", _breedEntry);
        AddField(petFields, "Edad (meses)", _ageEntry);
        AddField(petFields, "Sexo", _sexEntry);
        AddField(petFields, "Tamaño", _sizeEntry);
        AddField(petFields, "Ubicación", _locationEntry);
        layout.Children.Add(new Border { Padding = 12, Content = petFields });

        // Anuncio
        layout.Children.Add(new Label { Text = "Anuncio", Style = Res("SectionTitle") });
        var annFields = new VerticalStackLayout { Spacing = 8 };
        AddField(annFields, "Tipo de anuncio", _typePicker);
        AddField(annFields, "Estado", _statusPicker);
        layout.Children.Add(new Border { Padding = 12, Content = annFields });

        // Contacto
        layout.Children.Add(new Label { Text = "Contacto", Style = Res("SectionTitle") });
        var contactFields = new VerticalStackLayout { Spacing = 8 };
        AddField(contactFields, "Responsable", _responsibleEntry);
        AddField(contactFields, "Teléfono", _phoneEntry);
        AddField(contactFields, "Fundación", _foundationPicker);
        layout.Children.Add(new Border { Padding = 12, Content = contactFields });

        // Descripción
        layout.Children.Add(new Label { Text = "Descripción", Style = Res("SectionTitle") });
        layout.Children.Add(new Border { Padding = 12, Content = _descriptionEntry });

        var saveBtn = new Button { Text = "Publicar" };
        saveBtn.Clicked += OnSaveClicked;
        var cancelBtn = new Button { Text = "Cancelar", Style = Res("SecondaryButton") };
        cancelBtn.Clicked += async (s, e) => await SafeGoToAsync("..");

        var buttons = new HorizontalStackLayout { Spacing = 8 };
        buttons.Children.Add(saveBtn);
        buttons.Children.Add(cancelBtn);
        layout.Children.Add(buttons);

        Content = new ScrollView { Content = layout };
    }

    private static void AddField(VerticalStackLayout parent, string label, View control)
    {
        parent.Children.Add(new Label { Text = label, Style = Res("MutedText"), FontSize = 12 });
        parent.Children.Add(control);
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (_catalog is null)
        {
            await DisplayAlertAsync("Error", "Servicio no disponible", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(_nameEntry.Text))
        {
            await DisplayAlertAsync("Validación", "Por favor ingresa un nombre.", "OK");
            return;
        }
        if (string.IsNullOrWhiteSpace(_speciesEntry.Text))
        {
            await DisplayAlertAsync("Validación", "Por favor ingresa la especie.", "OK");
            return;
        }

        if (!int.TryParse(_ageEntry.Text, out var ageMonths))
            ageMonths = 0;

        Foundation? foundation = null;
        if (_foundationPicker.SelectedIndex > 0 && _catalog.GetFoundations().Count > 0)
            foundation = _catalog.GetFoundations()[_foundationPicker.SelectedIndex - 1];

        var pet = new Pet
        {
            Name = _nameEntry.Text.Trim(),
            Species = _speciesEntry.Text.Trim(),
            Breed = _breedEntry.Text?.Trim() ?? string.Empty,
            AgeMonths = ageMonths,
            Sex = _sexEntry.Text?.Trim() ?? string.Empty,
            Size = _sizeEntry.Text?.Trim() ?? string.Empty,
            Location = _locationEntry.Text?.Trim() ?? string.Empty,
            Description = _descriptionEntry.Text?.Trim() ?? string.Empty,
            ImageUrl = string.Empty,
            ListingType = Enum.Parse<ListingType>(_typePicker.SelectedItem.ToString()!),
            Status = Enum.Parse<PetStatus>(_statusPicker.SelectedItem.ToString()!),
            ResponsibleName = string.IsNullOrWhiteSpace(_responsibleEntry.Text) ? _catalog.CurrentUser.Name : _responsibleEntry.Text.Trim(),
            ContactPhone = string.IsNullOrWhiteSpace(_phoneEntry.Text) ? _catalog.CurrentUser.Phone : _phoneEntry.Text.Trim(),
            Foundation = foundation
        };

        _catalog.AddPet(pet);

        await DisplayAlertAsync("Listo", "Mascota publicada correctamente.", "OK");
        // Navigate back to the corresponding listing
        if (pet.ListingType == ListingType.Adoption)
            await SafeGoToAsync("//adoptions");
        else if (pet.ListingType == ListingType.Lost)
            await SafeGoToAsync("//lost");
        else if (pet.ListingType == ListingType.Found)
            await SafeGoToAsync("//found");
    }

    private static Style Res(string key) => (Style)Application.Current!.Resources[key];

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
}
