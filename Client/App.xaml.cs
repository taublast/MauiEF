namespace MauiEF.Client;

public partial class App : Application
{
	public static IServiceProvider Services { get; protected set; }

	public App(IServiceProvider services)
	{
		InitializeComponent();

		Services = services;

		MainPage = new AppShell();
	}
}
