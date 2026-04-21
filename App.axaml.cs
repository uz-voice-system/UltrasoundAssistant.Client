using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using UltrasoundAssistant.DoctorClient.Services;
using UltrasoundAssistant.DoctorClient.Services.AudioService;
using UltrasoundAssistant.DoctorClient.ViewModels;
using UltrasoundAssistant.DoctorClient.Views;

namespace UltrasoundAssistant.DoctorClient;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = default!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5000/")
        });
        services.AddSingleton<IAudioRecorderService, PortAudioRecorderService>();

        services.AddSingleton<AuthApiService>();
        services.AddSingleton<PatientApiService>();
        services.AddSingleton<TemplateApiService>();
        services.AddSingleton<ReportApiService>();
        services.AddSingleton<VoiceApiService>();

        services.AddSingleton<MainWindowViewModel>();
    }
}