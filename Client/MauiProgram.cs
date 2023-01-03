using MauiEF.Shared.Services;
using Microsoft.Extensions.Logging;

namespace MauiEF.Client;

public static class MauiProgram
{
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

		builder.Services.AddTransient<LocalDatabase>((services) =>
		{
			return new LocalDatabase(Path.Combine(FileSystem.AppDataDirectory, "SQLite001.db3"));
		});

		return builder.Build();
	}
}
