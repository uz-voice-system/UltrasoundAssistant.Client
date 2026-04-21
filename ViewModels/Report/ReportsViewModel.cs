using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;
using UltrasoundAssistant.DoctorClient.Helpers;
using UltrasoundAssistant.DoctorClient.Models.Commands.Report;
using UltrasoundAssistant.DoctorClient.Models.Read.Report;
using UltrasoundAssistant.DoctorClient.Services;
using UltrasoundAssistant.DoctorClient.Services.AudioService;

namespace UltrasoundAssistant.DoctorClient.ViewModels.Report;

public partial class ReportsViewModel : ViewModelBase
{
    private readonly TemplateApiService _templateService;
    private readonly ReportApiService _reportService;
    private readonly VoiceApiService _voiceApiService;
    private readonly IAudioRecorderService _audioRecorderService;
    private MainWindowViewModel _main { get; }

    public ObservableCollection<ReportDto> Reports { get; } = new();
    public ObservableCollection<string> Statuses { get; } = new();

    private string? _selectedStatus;
    public string? SelectedStatus
    {
        get => _selectedStatus;
        set => SetProperty(ref _selectedStatus, value);
    }

    private ReportDto? _selectedReport;
    public ReportDto? SelectedReport
    {
        get => _selectedReport;
        set
        {
            if (SetProperty(ref _selectedReport, value))
            {
                OnPropertyChanged(nameof(SelectedReportContentText));
            }
        }
    }

    public string SelectedReportContentText =>
        SelectedReport == null || SelectedReport.Content == null || SelectedReport.Content.Count == 0
            ? string.Empty
            : string.Join(Environment.NewLine, SelectedReport.Content.Select(x => $"{x.Key}: {x.Value}"));

    private bool _isDetailsPanelVisible;
    public bool IsDetailsPanelVisible
    {
        get => _isDetailsPanelVisible;
        set => SetProperty(ref _isDetailsPanelVisible, value);
    }

    [ObservableProperty]
    private string? errorMessage;

    public ICommand LoadReportsCommand { get; }
    public ICommand FilterReportsCommand { get; }
    public ICommand PreviewReportCommand { get; }
    public ICommand EditReportCommand { get; }
    public ICommand DeleteReportCommand { get; }
    public ICommand CloseDetailsCommand { get; }
    public ICommand GoBackCommand { get; }

    public ReportsViewModel(
        MainWindowViewModel main,
        TemplateApiService templateApiService,
        ReportApiService reportApiService,
        VoiceApiService voiceApiService,
        IAudioRecorderService audioRecorderService)
    {
        _main = main;
        _templateService = templateApiService;
        _reportService = reportApiService;
        _voiceApiService = voiceApiService;
        _audioRecorderService = audioRecorderService;

        Statuses.Add("Все");
        Statuses.Add("Черновик");
        Statuses.Add("Завершён");

        SelectedStatus = Statuses[0];

        LoadReportsCommand = new AsyncRelayCommand(LoadReportsAsync);
        FilterReportsCommand = new AsyncRelayCommand(FilterReportsAsync);
        PreviewReportCommand = new RelayCommand<ReportDto?>(OpenDetails);
        EditReportCommand = new RelayCommand<ReportDto?>(async r => await OpenReportAsync(r));
        DeleteReportCommand = new RelayCommand<ReportDto?>(async r => await DeleteReportAsync(r));
        CloseDetailsCommand = new RelayCommandSync(_ => CloseDetails());
        GoBackCommand = main.GoBackCommand;
    }

    private async Task LoadReportsAsync()
    {
        ErrorMessage = null;
        Reports.Clear();
        IsDetailsPanelVisible = false;
        SelectedReport = null;

        var result = await _reportService.GetAllAsync();

        if (result.IsSuccess && result.Data != null)
        {
            foreach (var report in result.Data)
                Reports.Add(report);
        }
        else
        {
            ErrorMessage = result.ErrorMessage;
        }
    }

    private async Task FilterReportsAsync()
    {
        ErrorMessage = null;
        Reports.Clear();
        IsDetailsPanelVisible = false;
        SelectedReport = null;

        var result = await _reportService.GetAllAsync();

        if (!result.IsSuccess || result.Data == null)
        {
            ErrorMessage = result.ErrorMessage;
            return;
        }

        var reports = result.Data.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SelectedStatus) && SelectedStatus != "Все")
        {
            reports = SelectedStatus switch
            {
                "Черновик" => reports.Where(r => r.Status.ToString().Equals("Draft", StringComparison.OrdinalIgnoreCase)),
                "Завершён" => reports.Where(r => r.Status.ToString().Equals("Completed", StringComparison.OrdinalIgnoreCase)),
                _ => reports
            };
        }

        foreach (var report in reports)
            Reports.Add(report);
    }

    private void OpenDetails(ReportDto? report)
    {
        if (report == null)
            return;

        ErrorMessage = null;
        SelectedReport = report;
        IsDetailsPanelVisible = true;
    }

    private void CloseDetails()
    {
        SelectedReport = null;
        IsDetailsPanelVisible = false;
        OnPropertyChanged(nameof(SelectedReportContentText));
    }

    private async Task OpenReportAsync(ReportDto? report)
    {
        if (report == null)
            return;

        ErrorMessage = null;

        if (!report.Status.ToString().Equals("Draft", StringComparison.OrdinalIgnoreCase))
        {
            ErrorMessage = "Редактировать можно только черновики.";
            return;
        }

        var editorVm = new ReportEditorViewModel(
            _main,
            _reportService,
            _templateService,
            _voiceApiService,
            _audioRecorderService);

        await editorVm.LoadReportAsync(report.Id);
        _main.UpdateCurrentView(editorVm);
    }

    private async Task DeleteReportAsync(ReportDto? report)
    {
        ErrorMessage = null;

        if (report == null)
            return;

        var command = new DeleteReportCommand
        {
            CommandId = Guid.NewGuid(),
            ReportId = report.Id,
            ExpectedVersion = report.Version
        };

        var result = await _reportService.DeleteAsync(command);

        if (result.IsSuccess)
            await FilterReportsAsync();
        else
            ErrorMessage = result.ErrorMessage;
    }
}