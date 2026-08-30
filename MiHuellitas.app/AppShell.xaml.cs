namespace MiHuellitas.app;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		// register routes
		Routing.RegisterRoute("petdetail", typeof(PetDetailPage));
	}
}
