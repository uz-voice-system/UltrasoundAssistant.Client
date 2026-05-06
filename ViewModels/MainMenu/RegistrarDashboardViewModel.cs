using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using UltrasoundAssistant.DoctorClient.Helpers;
using UltrasoundAssistant.DoctorClient.Services;
using UltrasoundAssistant.DoctorClient.ViewModels.Appointment;
using UltrasoundAssistant.DoctorClient.ViewModels.Patient;

namespace UltrasoundAssistant.DoctorClient.ViewModels.MainMenu;

public class RegistrarDashboardViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _main;

    private readonly PatientsViewModel _patientsViewModel;
    private readonly AppointmentRegistrationViewModel _appointmentRegistrationViewModel;

    public ICommand LogoutCommand { get; }
    public ICommand OpenPatientsCommand { get; }
    public ICommand OpenAppointmentRegistrationCommand { get; }

    public RegistrarDashboardViewModel(
        MainWindowViewModel main,
        PatientApiService patientApiService,
        UserApiService userApiService,
        TemplateApiService templateApiService,
        AppointmentApiService appointmentApiService,
        ReportApiService reportApiService)
    {
        _main = main;

        _patientsViewModel = new PatientsViewModel(main, patientApiService);

        _appointmentRegistrationViewModel = new AppointmentRegistrationViewModel(
            main,
            patientApiService,
            userApiService,
            templateApiService,
            appointmentApiService,
            reportApiService);

        LogoutCommand = main.LogoutCommand;

        OpenPatientsCommand = new RelayCommandSync(_ => OpenPatientsView());
        OpenAppointmentRegistrationCommand = new RelayCommandSync(_ => OpenAppointmentRegistrationView());
    }

    private void OpenPatientsView()
    {
        _main.UpdateCurrentView(_patientsViewModel);
    }

    private void OpenAppointmentRegistrationView()
    {
        _main.UpdateCurrentView(_appointmentRegistrationViewModel);
    }
}
