using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using UltrasoundAssistant.DoctorClient.Services;
using UltrasoundAssistant.DoctorClient.Services.AudioService;
using UltrasoundAssistant.DoctorClient.ViewModels.Report;

namespace UltrasoundAssistant.DoctorClient.ViewModels.MainMenu;

public class DoctorDashboardViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _main;
    private readonly CreateReportViewModel _createReportViewModel;
    private readonly ReportsViewModel _reportsViewModel;

    public ICommand LogoutCommand { get; }
    public ICommand OpenCreateReportCommand { get; }
    public ICommand OpenReportsCommand { get; }

    public DoctorDashboardViewModel(
        MainWindowViewModel main,
        ReportApiService reportApiService,
        PatientApiService patientApiService,
        TemplateApiService templateApiService,
        VoiceApiService voiceApiService,
        IAudioRecorderService audioRecorderService)
    {
        _main = main;

        _createReportViewModel = new CreateReportViewModel(main, patientApiService, templateApiService, reportApiService, voiceApiService, audioRecorderService);
        _reportsViewModel = new ReportsViewModel(main, templateApiService, reportApiService, voiceApiService, audioRecorderService);

        LogoutCommand = main.LogoutCommand;
        OpenCreateReportCommand = new RelayCommand(OpenCreateReportView);
        OpenReportsCommand = new RelayCommand(OpenReportsView);
    }

    private void OpenCreateReportView()
    {
        _createReportViewModel.ResetForNewReport();
        _main.UpdateCurrentView(_createReportViewModel);
    }

    private void OpenReportsView()
    {
        _main.UpdateCurrentView(_reportsViewModel);
    }
}