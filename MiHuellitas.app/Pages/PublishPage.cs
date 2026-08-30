using Microsoft.Maui.Controls;
using MiHuellitas.shared.Services;
using MiHuellitas.shared.Models;
using MiHuellitas.shared.Enums;
using System;

namespace MiHuellitas.app.Pages;

public class PublishPage : ContentPage
{
    private Entry _nameEntry;
    private Entry _speciesEntry;
    private Picker _typePicker;
    private MockCatalogService? _catalog;

    public PublishPage()
    {
        Title = "Publicar";
        _catalog = MauiProgram.Services?.GetService(typeof(MockCatalogService)) as MockCatalogService;

        var layout = new VerticalStackLayout { Padding = 12, Spacing = 10 };

        _nameEntry = new Entry { Placeholder = "Nombre" };
        _speciesEntry = new Entry { Placeholder = "Especie" };
        _typePicker = new Picker { Title = "Tipo" };
        _typePicker.Items.Add(ListingType.Adoption.ToString());
        _typePicker.Items.Add(ListingType.Lost.ToString());
        _typePicker.Items.Add(ListingType.Found.ToString());

        var saveBtn = new Button { Text = "Publicar" };
        saveBtn.Clicked += OnSaveClicked;

        layout.Children.Add(_nameEntry);
        layout.Children.Add(_speciesEntry);
        layout.Children.Add(_typePicker);
        layout.Children.Add(saveBtn);

        Content = new ScrollView { Content = layout };
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (_catalog is null)
        {
            await DisplayAlert("Error", "Servicio no disponible", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(_nameEntry.Text) || string.IsNullOrWhiteSpace(_speciesEntry.Text) || _typePicker.SelectedIndex < 0)
        {
            await DisplayAlert("Validación", "Nombre, especie y tipo son requeridos.", "OK");
            return;
        }

        var pet = new Pet
        {
            Name = _nameEntry.Text.Trim(),
            Species = _speciesEntry.Text.Trim(),
            Breed = string.Empty,
            AgeMonths = 0,
            Sex = string.Empty,
            Size = string.Empty,
            Location = "",
            Description = string.Empty,
            ImageUrl = string.Empty,
            ListingType = Enum.Parse<ListingType>(_typePicker.SelectedItem.ToString()!),
            Status = PetStatus.Available,
            ResponsibleName = _catalog.CurrentUser.Name,
            ContactPhone = _catalog.CurrentUser.Phone
        };

        _catalog.AddPet(pet);

        await DisplayAlert("Listo", "Mascota publicada (mock)", "OK");
        // Navigate back to the corresponding listing
        if (pet.ListingType == ListingType.Adoption)
            await Shell.Current.GoToAsync("/adoptions");
        else if (pet.ListingType == ListingType.Lost)
            await Shell.Current.GoToAsync("/lost");
        else if (pet.ListingType == ListingType.Found)
            await Shell.Current.GoToAsync("/found");
    }
}
