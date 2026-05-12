using System.Windows.Input;
using UltrasoundAssistant.DoctorClient.Helpers;
using UltrasoundAssistant.DoctorClient.Services;
using UltrasoundAssistant.DoctorClient.ViewModels.Template;
using UltrasoundAssistant.DoctorClient.ViewModels.User;

namespace UltrasoundAssistant.DoctorClient.ViewModels.MainMenu;

public class AdminDashboardViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _main;

    private readonly TemplatesAdminViewModel _templatesViewModel;
    private readonly UsersAdminViewModel _usersViewModel;

    public ICommand LogoutCommand { get; }
    public ICommand OpenTemplatesCommand { get; }
    public ICommand OpenUsersCommand { get; }

    public AdminDashboardViewModel(
        MainWindowViewModel main,
        TemplateApiService templateApiService,
        UserApiService userApiService,
        ScheduleApiService scheduleApiService)
    {
        _main = main;

        _templatesViewModel = new TemplatesAdminViewModel(main, templateApiService);
        _usersViewModel = new UsersAdminViewModel(main, userApiService, scheduleApiService);

        LogoutCommand = main.LogoutCommand;
        OpenTemplatesCommand = new RelayCommandSync(_ => OpenTemplatesView());
        OpenUsersCommand = new RelayCommandSync(_ => OpenUsersView());
    }

    private void OpenTemplatesView()
    {
        _main.UpdateCurrentView(_templatesViewModel);
    }

    private void OpenUsersView()
    {
        _main.UpdateCurrentView(_usersViewModel);
    }
}
