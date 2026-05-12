using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using UltrasoundAssistant.DoctorClient.Helpers;
using UltrasoundAssistant.DoctorClient.Models.Enums;
using UltrasoundAssistant.DoctorClient.Models.Reads.Templates.Search;
using UltrasoundAssistant.DoctorClient.Models.Reads.Users.Search;
using UltrasoundAssistant.DoctorClient.Models.Statistics;
using UltrasoundAssistant.DoctorClient.Services;

namespace UltrasoundAssistant.DoctorClient.ViewModels.Statistics;

public class AdminStatisticsViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _main;
    private readonly ReportApiService _reportService;
    private readonly UserApiService _userService;
    private readonly TemplateApiService _templateService;

    public ObservableCollection<UserSummaryDto> Doctors { get; } = new();
    public ObservableCollection<TemplateSummaryDto> Templates { get; } = new();

    public ObservableCollection<DoctorStatisticsDto> DoctorRows { get; } = new();
    public ObservableCollection<TemplateStatisticsDto> TemplateRows { get; } = new();
    public ObservableCollection<AppointmentStatusStatisticsViewItem> AppointmentStatusRows { get; } = new();
    public ObservableCollection<ReportStatusStatisticsViewItem> ReportStatusRows { get; } = new();

    private string _dateFromText = DateTime.Today.AddDays(-30).ToString("dd.MM.yyyy");
    public string DateFromText
    {
        get => _dateFromText;
        set => SetProperty(ref _dateFromText, value);
    }

    private string _dateToText = DateTime.Today.ToString("dd.MM.yyyy");
    public string DateToText
    {
        get => _dateToText;
        set => SetProperty(ref _dateToText, value);
    }

    private UserSummaryDto? _selectedDoctor;
    public UserSummaryDto? SelectedDoctor
    {
        get => _selectedDoctor;
        set => SetProperty(ref _selectedDoctor, value);
    }

    private TemplateSummaryDto? _selectedTemplate;
    public TemplateSummaryDto? SelectedTemplate
    {
        get => _selectedTemplate;
        set => SetProperty(ref _selectedTemplate, value);
    }

    private AdminStatisticsDto? _statistics;
    public AdminStatisticsDto? Statistics
    {
        get => _statistics;
        set
        {
            if (SetProperty(ref _statistics, value))
                RaiseStatisticsProperties();
        }
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
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

    public bool HasStatistics => Statistics != null;

    public string PeriodText
    {
        get
        {
            if (Statistics == null)
                return "Период не загружен";

            return $"{Statistics.DateFromUtc:dd.MM.yyyy} — {Statistics.DateToUtc:dd.MM.yyyy}";
        }
    }

    public string TotalAppointmentsCountText => Statistics?.TotalAppointmentsCount.ToString() ?? "0";
    public string AcceptedAppointmentsCountText => Statistics?.AcceptedAppointmentsCount.ToString() ?? "0";
    public string UniqueAcceptedPatientsCountText => Statistics?.UniqueAcceptedPatientsCount.ToString() ?? "0";
    public string ReportsCountText => Statistics?.ReportsCount.ToString() ?? "0";
    public string AppointmentsWithoutReportCountText => Statistics?.AppointmentsWithoutReportCount.ToString() ?? "0";

    public string ScheduledAppointmentsCountText => Statistics?.ScheduledAppointmentsCount.ToString() ?? "0";
    public string InProgressAppointmentsCountText => Statistics?.InProgressAppointmentsCount.ToString() ?? "0";
    public string CompletedAppointmentsCountText => Statistics?.CompletedAppointmentsCount.ToString() ?? "0";
    public string NoShowAppointmentsCountText => Statistics?.NoShowAppointmentsCount.ToString() ?? "0";

    public ICommand LoadDictionariesCommand { get; }
    public ICommand LoadStatisticsCommand { get; }
    public ICommand DownloadPdfCommand { get; }
    public ICommand ClearFiltersCommand { get; }

    public ICommand SetTodayCommand { get; }
    public ICommand SetLast30DaysCommand { get; }
    public ICommand SetCurrentMonthCommand { get; }
    public ICommand SetPreviousMonthCommand { get; }

    public ICommand GoBackCommand { get; }

    public AdminStatisticsViewModel(
        MainWindowViewModel main,
        ReportApiService reportService,
        UserApiService userService,
        TemplateApiService templateService)
    {
        _main = main;
        _reportService = reportService;
        _userService = userService;
        _templateService = templateService;

        LoadDictionariesCommand = new AsyncRelayCommand(LoadDictionariesAsync);
        LoadStatisticsCommand = new AsyncRelayCommand(LoadStatisticsAsync);
        DownloadPdfCommand = new AsyncRelayCommand(DownloadPdfAsync);
        ClearFiltersCommand = new AsyncRelayCommand(ClearFiltersAsync);

        SetTodayCommand = new RelayCommandSync(_ => SetToday());
        SetLast30DaysCommand = new RelayCommandSync(_ => SetLast30Days());
        SetCurrentMonthCommand = new RelayCommandSync(_ => SetCurrentMonth());
        SetPreviousMonthCommand = new RelayCommandSync(_ => SetPreviousMonth());

        GoBackCommand = main.GoBackCommand;
    }

    public async Task InitializeAsync()
    {
        await LoadDictionariesAsync();
        await LoadStatisticsAsync();
    }

    private async Task LoadDictionariesAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;

        Doctors.Clear();
        Templates.Clear();

        Doctors.Add(new UserSummaryDto
        {
            Id = Guid.Empty,
            FullName = "Все врачи",
            Role = UserRole.Doctor,
            IsActive = true
        });

        Templates.Add(new TemplateSummaryDto
        {
            Id = Guid.Empty,
            Name = "Все шаблоны"
        });

        var doctorsResult = await _userService.SearchAsync(new UserSearchRequest
        {
            Role = UserRole.Doctor,
            IsActive = true
        });

        if (doctorsResult.IsSuccess && doctorsResult.Data != null)
        {
            foreach (var doctor in doctorsResult.Data.OrderBy(x => x.FullName))
                Doctors.Add(doctor);
        }

        var templatesResult = await _templateService.SearchForDoctorAsync(new TemplateSearchRequest
        {
            IncludeDeleted = false
        });

        if (templatesResult.IsSuccess && templatesResult.Data != null)
        {
            foreach (var template in templatesResult.Data.OrderBy(x => x.Name))
                Templates.Add(template);
        }

        SelectedDoctor = Doctors.FirstOrDefault();
        SelectedTemplate = Templates.FirstOrDefault();
    }

    private async Task LoadStatisticsAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;

        var request = BuildRequest();

        if (request == null)
            return;

        IsBusy = true;

        try
        {
            var result = await _reportService.GetAdminStatisticsAsync(request);

            if (!result.IsSuccess || result.Data == null)
            {
                ErrorMessage = result.ErrorMessage ?? "Не удалось получить статистику.";
                return;
            }

            ApplyStatistics(result.Data);
            SuccessMessage = "Статистика загружена.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DownloadPdfAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;

        var request = BuildRequest();

        if (request == null)
            return;

        IsBusy = true;

        try
        {
            var result = await _reportService.GetAdminStatisticsPdfAsync(request);

            if (!result.IsSuccess || result.Data == null)
            {
                ErrorMessage = result.ErrorMessage ?? "Не удалось сформировать PDF статистики.";
                return;
            }

            var filePath = Path.Combine(Path.GetTempPath(), result.Data.FileName);

            await File.WriteAllBytesAsync(filePath, result.Data.Content);

            ReportApiService.OpenFile(filePath);

            SuccessMessage = "PDF статистики сформирован и открыт.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ClearFiltersAsync()
    {
        SetLast30Days();

        SelectedDoctor = Doctors.FirstOrDefault();
        SelectedTemplate = Templates.FirstOrDefault();

        await LoadStatisticsAsync();
    }

    private AdminStatisticsRequest? BuildRequest()
    {
        if (!TryParseDate(DateFromText, out var dateFrom))
        {
            ErrorMessage = "Некорректная дата начала периода. Используйте формат дд.мм.гггг.";
            return null;
        }

        if (!TryParseDate(DateToText, out var dateTo))
        {
            ErrorMessage = "Некорректная дата окончания периода. Используйте формат дд.мм.гггг.";
            return null;
        }

        if (dateFrom > dateTo)
        {
            ErrorMessage = "Дата начала периода не может быть позже даты окончания.";
            return null;
        }

        return new AdminStatisticsRequest
        {
            DateFromUtc = ToUtcStartOfSelectedDate(dateFrom),
            DateToUtc = ToUtcEndOfSelectedDate(dateTo),
            DoctorId = SelectedDoctor == null || SelectedDoctor.Id == Guid.Empty
                ? null
                : SelectedDoctor.Id,
            TemplateId = SelectedTemplate == null || SelectedTemplate.Id == Guid.Empty
                ? null
                : SelectedTemplate.Id
        };
    }

    private void ApplyStatistics(AdminStatisticsDto statistics)
    {
        Statistics = statistics;

        DoctorRows.Clear();
        TemplateRows.Clear();
        AppointmentStatusRows.Clear();
        ReportStatusRows.Clear();

        foreach (var doctor in statistics.Doctors.OrderByDescending(x => x.ReportsCount))
            DoctorRows.Add(doctor);

        foreach (var template in statistics.Templates.OrderByDescending(x => x.ReportsCount))
            TemplateRows.Add(template);

        foreach (var status in statistics.AppointmentStatuses.OrderByDescending(x => x.Count))
            AppointmentStatusRows.Add(new AppointmentStatusStatisticsViewItem(status));

        foreach (var status in statistics.ReportStatuses.OrderByDescending(x => x.Count))
            ReportStatusRows.Add(new ReportStatusStatisticsViewItem(status));
    }

    private void SetToday()
    {
        var today = DateTime.Today;

        DateFromText = FormatDate(today);
        DateToText = FormatDate(today);
    }

    private void SetLast30Days()
    {
        var today = DateTime.Today;

        DateFromText = FormatDate(today.AddDays(-30));
        DateToText = FormatDate(today);
    }

    private void SetCurrentMonth()
    {
        var today = DateTime.Today;
        var firstDay = new DateTime(today.Year, today.Month, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);

        DateFromText = FormatDate(firstDay);
        DateToText = FormatDate(lastDay);
    }

    private void SetPreviousMonth()
    {
        var today = DateTime.Today;
        var currentMonthFirstDay = new DateTime(today.Year, today.Month, 1);
        var previousMonthFirstDay = currentMonthFirstDay.AddMonths(-1);
        var previousMonthLastDay = currentMonthFirstDay.AddDays(-1);

        DateFromText = FormatDate(previousMonthFirstDay);
        DateToText = FormatDate(previousMonthLastDay);
    }

    private void RaiseStatisticsProperties()
    {
        OnPropertyChanged(nameof(HasStatistics));
        OnPropertyChanged(nameof(PeriodText));

        OnPropertyChanged(nameof(TotalAppointmentsCountText));
        OnPropertyChanged(nameof(AcceptedAppointmentsCountText));
        OnPropertyChanged(nameof(UniqueAcceptedPatientsCountText));
        OnPropertyChanged(nameof(ReportsCountText));
        OnPropertyChanged(nameof(AppointmentsWithoutReportCountText));

        OnPropertyChanged(nameof(ScheduledAppointmentsCountText));
        OnPropertyChanged(nameof(InProgressAppointmentsCountText));
        OnPropertyChanged(nameof(CompletedAppointmentsCountText));
        OnPropertyChanged(nameof(NoShowAppointmentsCountText));
    }

    private static bool TryParseDate(string text, out DateTime value)
    {
        return DateTime.TryParseExact(
            text?.Trim(),
            "dd.MM.yyyy",
            CultureInfo.GetCultureInfo("ru-RU"),
            DateTimeStyles.None,
            out value);
    }

    private static string FormatDate(DateTime date)
    {
        return date.ToString("dd.MM.yyyy");
    }

    private static DateTime ToUtcStartOfSelectedDate(DateTime selectedDate)
    {
        return new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, 0, 0, 0, DateTimeKind.Utc);
    }

    private static DateTime ToUtcEndOfSelectedDate(DateTime selectedDate)
    {
        return new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, 23, 59, 59, DateTimeKind.Utc);
    }
}
