using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MiHuellitas.shared.Services;
using System;

namespace MiHuellitas.app;

public static class MauiProgram
{
	public static IServiceProvider? Services { get; private set; }

	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Register shared services for the app
		builder.Services.AddSingleton<MockCatalogService>();

		var app = builder.Build();
		Services = app.Services;
		return app;
	}
}
