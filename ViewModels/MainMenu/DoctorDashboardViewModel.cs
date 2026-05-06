using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;
using UltrasoundAssistant.DoctorClient.Helpers;
using UltrasoundAssistant.DoctorClient.Models.Commands.Appointments;
using UltrasoundAssistant.DoctorClient.Models.Enums;
using UltrasoundAssistant.DoctorClient.Models.Reads.Appointments.Details;
using UltrasoundAssistant.DoctorClient.Models.Reads.Appointments.Search;
using UltrasoundAssistant.DoctorClient.Services;
using UltrasoundAssistant.DoctorClient.Services.AudioService;
using UltrasoundAssistant.DoctorClient.ViewModels.Report;

namespace UltrasoundAssistant.DoctorClient.ViewModels.MainMenu;

public class DoctorDashboardViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _main;
    private readonly AppointmentApiService _appointmentService;
    private readonly ReportApiService _reportService;
    private readonly TemplateApiService _templateService;
    private readonly VoiceApiService _voiceApiService;
    private readonly IAudioRecorderService _audioRecorderService;

    public ObservableCollection<AppointmentSummaryDto> TodayAppointments { get; } = new();
    public ObservableCollection<AppointmentStatus?> Statuses { get; } = new();

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private DateTimeOffset? _filterFromDate = DateTimeOffset.Now.Date;
    public DateTimeOffset? FilterFromDate
    {
        get => _filterFromDate;
        set => SetProperty(ref _filterFromDate, value);
    }

    private DateTimeOffset? _filterToDate = DateTimeOffset.Now.Date;
    public DateTimeOffset? FilterToDate
    {
        get => _filterToDate;
        set => SetProperty(ref _filterToDate, value);
    }

    private AppointmentStatus? _selectedStatus;
    public AppointmentStatus? SelectedStatus
    {
        get => _selectedStatus;
        set => SetProperty(ref _selectedStatus, value);
    }

    private bool _areAdditionalFiltersVisible;
    public bool AreAdditionalFiltersVisible
    {
        get => _areAdditionalFiltersVisible;
        set => SetProperty(ref _areAdditionalFiltersVisible, value);
    }

    private bool _includeDeleted;
    public bool IncludeDeleted
    {
        get => _includeDeleted;
        set => SetProperty(ref _includeDeleted, value);
    }

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    private string? _successMessage;
    public string? SuccessMessage
    {
        get => _successMessage;
        set => SetProperty(ref _successMessage, value);
    }

    public ICommand LogoutCommand { get; }
    public ICommand LoadTodayAppointmentsCommand { get; }
    public ICommand SearchAppointmentsCommand { get; }
    public ICommand ClearFiltersCommand { get; }
    public ICommand ToggleAdditionalFiltersCommand { get; }
    public ICommand OpenReportCommand { get; }
    public ICommand MarkNoShowCommand { get; }
    public ICommand OpenReportsArchiveCommand { get; }

    public DoctorDashboardViewModel(
        MainWindowViewModel main,
        AppointmentApiService appointmentApiService,
        ReportApiService reportApiService,
        TemplateApiService templateApiService,
        VoiceApiService voiceApiService,
        IAudioRecorderService audioRecorderService)
    {
        _main = main;
        _appointmentService = appointmentApiService;
        _reportService = reportApiService;
        _templateService = templateApiService;
        _voiceApiService = voiceApiService;
        _audioRecorderService = audioRecorderService;

        Statuses.Add(null);
        foreach (var status in Enum.GetValues<AppointmentStatus>())
            Statuses.Add(status);

        LogoutCommand = main.LogoutCommand;

        LoadTodayAppointmentsCommand = new AsyncRelayCommand(LoadTodayAppointmentsAsync);
        SearchAppointmentsCommand = new AsyncRelayCommand(SearchAppointmentsAsync);
        ClearFiltersCommand = new AsyncRelayCommand(ClearFiltersAsync);

        ToggleAdditionalFiltersCommand = new RelayCommandSync(_ =>
        {
            AreAdditionalFiltersVisible = !AreAdditionalFiltersVisible;
        });

        OpenReportCommand = new RelayCommand<AppointmentSummaryDto?>(async a =>
        {
            await OpenReportAsync(a);
        });

        MarkNoShowCommand = new RelayCommand<AppointmentSummaryDto?>(async a =>
        {
            await MarkNoShowAsync(a);
        });

        OpenReportsArchiveCommand = new RelayCommandSync(_ => OpenReportsArchive());
    }

    private async Task LoadTodayAppointmentsAsync()
    {
        FilterFromDate = DateTimeOffset.Now.Date;
        FilterToDate = DateTimeOffset.Now.Date;

        await SearchAppointmentsAsync();
    }

    private async Task SearchAppointmentsAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;
        TodayAppointments.Clear();

        var from = FilterFromDate?.DateTime.Date ?? DateTime.Now.Date;
        var to = (FilterToDate?.DateTime.Date ?? from).AddDays(1);

        var filter = new AppointmentSearchRequest
        {
            DoctorId = _main.CurrentUser.UserId,
            SearchText = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim(),
            FromUtc = from.ToUniversalTime(),
            ToUtc = to.ToUniversalTime(),
            Status = AreAdditionalFiltersVisible ? SelectedStatus : null,
            IncludeDeleted = IncludeDeleted
        };

        var result = await _appointmentService.SearchAsync(filter);

        if (!result.IsSuccess || result.Data == null)
        {
            ErrorMessage = result.ErrorMessage;
            return;
        }

        foreach (var appointment in result.Data.OrderBy(x => x.StartAtUtc))
            TodayAppointments.Add(appointment);
    }

    private async Task ClearFiltersAsync()
    {
        SearchText = string.Empty;
        FilterFromDate = DateTimeOffset.Now.Date;
        FilterToDate = DateTimeOffset.Now.Date;
        SelectedStatus = null;
        IncludeDeleted = false;

        await SearchAppointmentsAsync();
    }

    private async Task OpenReportAsync(AppointmentSummaryDto? appointment)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (appointment == null)
            return;

        var reportResult = await _reportService.GetByAppointmentIdAsync(appointment.Id);

        if (!reportResult.IsSuccess || reportResult.Data == null)
        {
            ErrorMessage = reportResult.ErrorMessage ?? "Для этой записи не найден отчёт.";
            return;
        }

        var editorVm = new ReportEditorViewModel(
            _main,
            _reportService,
            _templateService,
            _voiceApiService,
            _audioRecorderService);

        await editorVm.LoadReportAsync(reportResult.Data.Id);

        _main.UpdateCurrentView(editorVm);
    }

    private async Task MarkNoShowAsync(AppointmentSummaryDto? appointment)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (appointment == null)
            return;

        var fullResult = await _appointmentService.GetByIdAsync(appointment.Id);

        if (!fullResult.IsSuccess || fullResult.Data == null)
        {
            ErrorMessage = fullResult.ErrorMessage ?? "Не удалось загрузить запись на приём.";
            return;
        }

        var command = BuildNoShowCommand(fullResult.Data);

        var result = await _appointmentService.UpdateAsync(command);

        if (!result.IsSuccess)
        {
            ErrorMessage = result.ErrorMessage;
            return;
        }

        SuccessMessage = "Запись отмечена как неявка пациента.";
        await SearchAppointmentsAsync();
    }

    private static UpdateAppointmentCommand BuildNoShowCommand(AppointmentDto appointment)
    {
        return new UpdateAppointmentCommand
        {
            AppointmentId = appointment.Id,
            PatientId = appointment.PatientId,
            DoctorId = appointment.DoctorId,
            TemplateId = appointment.TemplateId,
            StartAtUtc = appointment.StartAtUtc,
            EndAtUtc = appointment.EndAtUtc,
            Status = AppointmentStatus.NoShow,
            Comment = appointment.Comment,
            ExpectedVersion = appointment.Version
        };
    }

    private void OpenReportsArchive()
    {
        var reportsVm = new DoctorReportsArchiveViewModel(
            _main,
            _reportService,
            _templateService,
            _voiceApiService,
            _audioRecorderService);

        _main.UpdateCurrentView(reportsVm);
    }
}