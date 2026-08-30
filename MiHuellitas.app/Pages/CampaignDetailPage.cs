using Microsoft.Maui.Controls;
using MiHuellitas.shared.Services;
using System;

namespace MiHuellitas.app.Pages;

[QueryProperty(nameof(CampaignId), "campaignId")]
public class CampaignDetailPage : ContentPage
{
    private MockCatalogService? _catalog;
    private int _campaignId;

    public string CampaignId
    {
        set
        {
            if (int.TryParse(value, out var id))
            {
                _campaignId = id;
                LoadCampaign();
            }
        }
    }

    public CampaignDetailPage()
    {
        Title = "Detalle campaña";
        _catalog = MauiProgram.Services?.GetService(typeof(MockCatalogService)) as MockCatalogService;
        Content = new VerticalStackLayout { Padding = 12, Children = { new Label { Text = "Cargando..." } } };
    }

    private void LoadCampaign()
    {
        if (_catalog is null)
        {
            Content = new Label { Text = "Servicio no disponible" };
            return;
        }

        var campaign = _catalog.GetCampaignById(_campaignId);
        if (campaign is null)
        {
            Content = new Label { Text = "Campaña no encontrada." };
            return;
        }

        var layout = new VerticalStackLayout { Padding = 12, Spacing = 8 };
        layout.Children.Add(new Label { Text = "Campaña", Style = Res("Eyebrow") });
        if (!string.IsNullOrEmpty(campaign.ImageUrl))
            layout.Children.Add(new Image { Source = campaign.ImageUrl, HeightRequest = 200, Aspect = Aspect.AspectFill });
        layout.Children.Add(new Label { Text = campaign.Title, Style = Res("PageTitle") });
        layout.Children.Add(new Label { Text = campaign.Organizer, Style = Res("MutedText") });
        layout.Children.Add(new Label { Text = campaign.Status, Style = Res("BadgeTextAccent") });
        layout.Children.Add(new Label { Text = $"Ubicación: {campaign.Location}", Style = Res("MutedText") });
        layout.Children.Add(new Label { Text = $"Fecha: {campaign.EventDate:dd MMM yyyy, HH:mm}", Style = Res("MutedText") });
        layout.Children.Add(new Label { Text = campaign.Description, Style = Res("MutedText") });

        var backBtn = new Button { Text = "Volver", Style = Res("SecondaryButton") };
        backBtn.Clicked += async (s, e) => await GoBackAsync();
        layout.Children.Add(backBtn);

        Content = new ScrollView { Content = layout };
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
