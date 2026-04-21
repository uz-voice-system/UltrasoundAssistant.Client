using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using UltrasoundAssistant.DoctorClient.Services;
using UltrasoundAssistant.DoctorClient.ViewModels.Template;

namespace UltrasoundAssistant.DoctorClient.ViewModels.MainMenu;

public class AdminDashboardViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _main;
    private readonly TemplatesViewModel _templatesViewModel;

    public ICommand LogoutCommand { get; }
    public ICommand OpenTemplatesCommand { get; }

    public AdminDashboardViewModel(
        MainWindowViewModel main,
        TemplateApiService templateApiService)
    {
        _main = main;
        _templatesViewModel = new TemplatesViewModel(main, templateApiService);

        LogoutCommand = main.LogoutCommand;
        OpenTemplatesCommand = new RelayCommand(OpenTemplatesView);
    }

    private void OpenTemplatesView()
    {
        _main.UpdateCurrentView(_templatesViewModel);
    }
}