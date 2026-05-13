using Avalonia.Media;
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

    public ObservableCollection<DoctorAppointmentCardItem> TodayAppointments { get; } = new();

    public ObservableCollection<string> StatusOptions { get; } = new();

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

    private string _selectedStatusText = "Все статусы записи";
    public string SelectedStatusText
    {
        get => _selectedStatusText;
        set => SetProperty(ref _selectedStatusText, value);
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

        StatusOptions.Add("Все статусы записи");
        StatusOptions.Add("Запланирована");
        StatusOptions.Add("В процессе");
        StatusOptions.Add("Завершена");
        StatusOptions.Add("Отменена");
        StatusOptions.Add("Неявка");

        SelectedStatusText = StatusOptions[0];

        LogoutCommand = main.LogoutCommand;

        LoadTodayAppointmentsCommand = new AsyncRelayCommand(LoadTodayAppointmentsAsync);
        SearchAppointmentsCommand = new AsyncRelayCommand(SearchAppointmentsAsync);
        ClearFiltersCommand = new AsyncRelayCommand(ClearFiltersAsync);

        ToggleAdditionalFiltersCommand = new RelayCommandSync(_ =>
        {
            AreAdditionalFiltersVisible = !AreAdditionalFiltersVisible;
        });

        OpenReportCommand = new RelayCommand<DoctorAppointmentCardItem?>(async item =>
        {
            await OpenReportAsync(item);
        });

        MarkNoShowCommand = new RelayCommand<DoctorAppointmentCardItem?>(async item =>
        {
            await MarkNoShowAsync(item);
        });

        OpenReportsArchiveCommand = new RelayCommandSync(_ => OpenReportsArchive());
    }

    public async Task InitializeAsync()
    {
        await LoadTodayAppointmentsAsync();
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

        if (FilterFromDate == null)
        {
            ErrorMessage = "Укажите дату начала периода.";
            return;
        }

        if (FilterToDate == null)
        {
            ErrorMessage = "Укажите дату окончания периода.";
            return;
        }

        var fromDate = FilterFromDate.Value.Date;
        var toDate = FilterToDate.Value.Date;

        if (fromDate > toDate)
        {
            ErrorMessage = "Дата начала периода не может быть позже даты окончания.";
            return;
        }

        var filter = new AppointmentSearchRequest
        {
            DoctorId = _main.CurrentUser.UserId,
            SearchText = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim(),
            FromUtc = ToUtcStartOfLocalDate(fromDate),
            ToUtc = ToUtcStartOfLocalDate(toDate.AddDays(1)),
            Status = AreAdditionalFiltersVisible ? MapNullableStatus(SelectedStatusText) : null,
            IncludeDeleted = IncludeDeleted
        };

        var result = await _appointmentService.SearchAsync(filter);

        if (!result.IsSuccess || result.Data == null)
        {
            ErrorMessage = result.ErrorMessage;
            return;
        }

        foreach (var appointment in result.Data.OrderBy(x => x.StartAtUtc))
            TodayAppointments.Add(new DoctorAppointmentCardItem(appointment));
    }

    private async Task ClearFiltersAsync()
    {
        SearchText = string.Empty;
        FilterFromDate = DateTimeOffset.Now.Date;
        FilterToDate = DateTimeOffset.Now.Date;
        SelectedStatusText = StatusOptions[0];
        IncludeDeleted = false;

        await SearchAppointmentsAsync();
    }

    private async Task OpenReportAsync(DoctorAppointmentCardItem? item)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (item == null)
            return;

        if (item.Status is AppointmentStatus.Canceled or AppointmentStatus.NoShow)
        {
            ErrorMessage = "Для отменённой записи или неявки отчёт заполнять нельзя.";
            return;
        }

        var reportResult = await _reportService.GetByAppointmentIdAsync(item.Id);

        if (!reportResult.IsSuccess || reportResult.Data == null)
        {
            ErrorMessage = reportResult.ErrorMessage ?? "Для этой записи не найден отчёт.";
            return;
        }

        var editorVm = new ReportEditorViewModel(
            _main,
            _appointmentService,
            _reportService,
            _templateService,
            _voiceApiService,
            _audioRecorderService);

        await editorVm.LoadReportAsync(reportResult.Data.Id);

        _main.UpdateCurrentView(editorVm);
    }

    private async Task MarkNoShowAsync(DoctorAppointmentCardItem? item)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (item == null)
            return;

        if (!item.CanMarkNoShow)
        {
            ErrorMessage = "Эту запись нельзя отметить как неявку.";
            return;
        }

        var fullResult = await _appointmentService.GetByIdAsync(item.Id);

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
            _appointmentService,
            _voiceApiService,
            _audioRecorderService);

        _main.UpdateCurrentView(reportsVm);
    }

    private static AppointmentStatus? MapNullableStatus(string? text)
    {
        return text switch
        {
            "Запланирована" => AppointmentStatus.Scheduled,
            "В процессе" => AppointmentStatus.InProgress,
            "Завершена" => AppointmentStatus.Completed,
            "Отменена" => AppointmentStatus.Canceled,
            "Неявка" => AppointmentStatus.NoShow,
            _ => null
        };
    }

    private static DateTime ToUtcStartOfLocalDate(DateTime date)
    {
        var localDateTime = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Local);
        return localDateTime.ToUniversalTime();
    }
}
