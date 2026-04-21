using System.Windows.Input;
using UltrasoundAssistant.DoctorClient.Helpers;
using UltrasoundAssistant.DoctorClient.Models;
using UltrasoundAssistant.DoctorClient.Models.Enum;
using UltrasoundAssistant.DoctorClient.Services;
using UltrasoundAssistant.DoctorClient.Services.AudioService;
using UltrasoundAssistant.DoctorClient.ViewModels.MainMenu;

namespace UltrasoundAssistant.DoctorClient.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private object _currentView = new();
    private LoginResponse? _currentUser = null;

    public object CurrentView
    {
        get => _currentView;
        set
        {
            _currentView = value;
            OnPropertyChanged();
        }
    }

    public LoginResponse CurrentUser
    {
        get => _currentUser!;
        set => _currentUser = value;
    }

    public ICommand GoBackCommand { get; }
    public ICommand LogoutCommand { get; }

    private ViewModelBase _mainMenuViewModel = new();
    private AdminDashboardViewModel _adminDashboardViewModel { get; }
    private DoctorDashboardViewModel _doctorDashboardViewModel { get; }
    private RegistrarDashboardViewModel _registrarDashboardViewModel { get; }
    private LoginViewModel _loginViewModel { get; }

    public MainWindowViewModel(
        AuthApiService authService,
        PatientApiService patientApiService,
        TemplateApiService templateApiService,
        ReportApiService reportApiService,
        VoiceApiService voiceApiService,
        IAudioRecorderService audioRecorderService)
    {
        _loginViewModel = new LoginViewModel(this, authService);

        GoBackCommand = new RelayCommand(async parameter => await GoBack());
        LogoutCommand = new RelayCommand(async parameter => await Logout());

        _adminDashboardViewModel = new AdminDashboardViewModel(
            this,
            templateApiService
        );

        _doctorDashboardViewModel = new DoctorDashboardViewModel(
            this,
            reportApiService,
            patientApiService,
            templateApiService,
            voiceApiService,
            audioRecorderService
        );

        _registrarDashboardViewModel = new RegistrarDashboardViewModel(
            this,
            patientApiService
        );

        CurrentView = _loginViewModel;
    }

    public void UpdateCurrentView(ViewModelBase viewModel)
    {
        CurrentView = viewModel;
        OnPropertyChanged(nameof(CurrentView));
    }

    public void OpenMainForRole(UserRole role)
    {
        if (role == UserRole.Admin)
            _mainMenuViewModel = _adminDashboardViewModel;

        else if (role == UserRole.Doctor)
            _mainMenuViewModel = _doctorDashboardViewModel;

        else if (role == UserRole.Registrar)
            _mainMenuViewModel = _registrarDashboardViewModel;

        UpdateCurrentView(_mainMenuViewModel);
    }

    public bool IsCurrentView(ViewModelBase vm)
    {
        return ReferenceEquals(CurrentView, vm);
    }

    private Task GoBack()
    {
        CurrentView = _mainMenuViewModel;
        OnPropertyChanged(nameof(CurrentView));
        return Task.CompletedTask;
    }

    private async Task Logout()
    {
        UpdateCurrentView(_loginViewModel);
        await Task.CompletedTask;
    }
}