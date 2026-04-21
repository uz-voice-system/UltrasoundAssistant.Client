using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using UltrasoundAssistant.DoctorClient.Services;
using UltrasoundAssistant.DoctorClient.ViewModels.Patient;

namespace UltrasoundAssistant.DoctorClient.ViewModels.MainMenu;

public class RegistrarDashboardViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _main;
    private readonly PatientsViewModel _patientsViewModel;

    public ICommand LogoutCommand { get; }
    public ICommand OpenPatientsCommand { get; }

    public RegistrarDashboardViewModel(
        MainWindowViewModel main,
        PatientApiService patientApiService)
    {
        _main = main;
        _patientsViewModel = new PatientsViewModel(main, patientApiService);

        LogoutCommand = main.LogoutCommand;
        OpenPatientsCommand = new RelayCommand(OpenPatientsView);
    }

    private void OpenPatientsView()
    {
        _main.UpdateCurrentView(_patientsViewModel);
    }
}