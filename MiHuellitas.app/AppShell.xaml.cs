namespace MiHuellitas.app;

using MiHuellitas.app.Pages;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		// register routes
		Routing.RegisterRoute("petdetail", typeof(PetDetailPage));
	}
}
