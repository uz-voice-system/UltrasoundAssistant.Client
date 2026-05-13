using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;
using UltrasoundAssistant.DoctorClient.Helpers;
using UltrasoundAssistant.DoctorClient.Models.Commands.Reports;
using UltrasoundAssistant.DoctorClient.Models.Enums;
using UltrasoundAssistant.DoctorClient.Models.Reads.Reports.Details;
using UltrasoundAssistant.DoctorClient.Models.Reads.Reports.Search;
using UltrasoundAssistant.DoctorClient.Models.Reads.Templates.Search;
using UltrasoundAssistant.DoctorClient.Services;
using UltrasoundAssistant.DoctorClient.Services.AudioService;

namespace UltrasoundAssistant.DoctorClient.ViewModels.Report;

public class DoctorReportsArchiveViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _main;
    private readonly ReportApiService _reportService;
    private readonly TemplateApiService _templateService;
    private readonly AppointmentApiService _appointmentService;
    private readonly VoiceApiService _voiceApiService;
    private readonly IAudioRecorderService _audioRecorderService;

    private Dictionary<string, string> _selectedReportFieldDisplayNames = new(StringComparer.OrdinalIgnoreCase);

    public ObservableCollection<DoctorReportArchiveListItem> Reports { get; } = new();
    public ObservableCollection<string> StatusOptions { get; } = new();
    public ObservableCollection<TemplateSummaryDto> Templates { get; } = new();

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private DateTimeOffset? _createdFromDate;
    public DateTimeOffset? CreatedFromDate
    {
        get => _createdFromDate;
        set => SetProperty(ref _createdFromDate, value);
    }

    private DateTimeOffset? _createdToDate;
    public DateTimeOffset? CreatedToDate
    {
        get => _createdToDate;
        set => SetProperty(ref _createdToDate, value);
    }

    private string _selectedStatusText = "Все статусы отчёта";
    public string SelectedStatusText
    {
        get => _selectedStatusText;
        set => SetProperty(ref _selectedStatusText, value);
    }

    private TemplateSummaryDto? _selectedTemplate;
    public TemplateSummaryDto? SelectedTemplate
    {
        get => _selectedTemplate;
        set => SetProperty(ref _selectedTemplate, value);
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

    private ReportDto? _selectedReport;
    public ReportDto? SelectedReport
    {
        get => _selectedReport;
        set
        {
            if (SetProperty(ref _selectedReport, value))
            {
                OnPropertyChanged(nameof(SelectedReportContentText));
                OnPropertyChanged(nameof(SelectedReportStatusText));
                OnPropertyChanged(nameof(SelectedReportAppointmentTimeText));
                OnPropertyChanged(nameof(SelectedReportImageText));
                OnPropertyChanged(nameof(HasSelectedReportImage));
            }
        }
    }

    private Bitmap? _selectedReportImage;
    public Bitmap? SelectedReportImage
    {
        get => _selectedReportImage;
        set
        {
            if (SetProperty(ref _selectedReportImage, value))
                OnPropertyChanged(nameof(HasSelectedReportImage));
        }
    }

    private bool _isDetailsPanelVisible;
    public bool IsDetailsPanelVisible
    {
        get => _isDetailsPanelVisible;
        set => SetProperty(ref _isDetailsPanelVisible, value);
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

    public bool HasReports => Reports.Count > 0;

    public string SelectedReportStatusText => SelectedReport == null
        ? string.Empty
        : GetReportStatusText(SelectedReport.Status);

    public string SelectedReportAppointmentTimeText
    {
        get
        {
            if (SelectedReport?.AppointmentStartAtUtc == null)
                return string.Empty;

            var start = ToLocalTime(SelectedReport.AppointmentStartAtUtc.Value);

            if (SelectedReport.AppointmentEndAtUtc == null)
                return start.ToString("dd.MM.yyyy HH:mm");

            var end = ToLocalTime(SelectedReport.AppointmentEndAtUtc.Value);

            return $"{start:dd.MM.yyyy HH:mm} — {end:HH:mm}";
        }
    }

    public bool HasSelectedReportImage => SelectedReportImage != null;

    public string SelectedReportImageText
    {
        get
        {
            if (SelectedReport == null)
                return string.Empty;

            if (!SelectedReport.HasUltrasoundImage)
                return "Изображение УЗИ не прикреплено.";

            if (SelectedReportImage == null)
                return "Изображение прикреплено, но его содержимое не пришло с сервера.";

            return string.IsNullOrWhiteSpace(SelectedReport.UltrasoundImageFileName)
                ? "Изображение УЗИ"
                : SelectedReport.UltrasoundImageFileName;
        }
    }

    public string SelectedReportContentText =>
        SelectedReport == null
            ? string.Empty
            : FormatContentJson(SelectedReport.ContentJson, _selectedReportFieldDisplayNames);

    public ICommand LoadReportsCommand { get; }
    public ICommand FilterReportsCommand { get; }
    public ICommand ClearFiltersCommand { get; }
    public ICommand ToggleAdditionalFiltersCommand { get; }
    public ICommand PreviewReportCommand { get; }
    public ICommand EditReportCommand { get; }
    public ICommand GeneratePdfCommand { get; }
    public ICommand ArchiveReportCommand { get; }
    public ICommand CloseDetailsCommand { get; }
    public ICommand GoBackCommand { get; }

    public DoctorReportsArchiveViewModel(
        MainWindowViewModel main,
        ReportApiService reportApiService,
        TemplateApiService templateApiService,
        AppointmentApiService appointmentApiService,
        VoiceApiService voiceApiService,
        IAudioRecorderService audioRecorderService)
    {
        _main = main;
        _reportService = reportApiService;
        _templateService = templateApiService;
        _appointmentService = appointmentApiService;
        _voiceApiService = voiceApiService;
        _audioRecorderService = audioRecorderService;

        StatusOptions.Add("Все статусы отчёта");
        StatusOptions.Add("Черновик");
        StatusOptions.Add("В процессе");
        StatusOptions.Add("Завершён");
        StatusOptions.Add("Архивирован");

        SelectedStatusText = StatusOptions[0];

        LoadReportsCommand = new AsyncRelayCommand(LoadReportsAsync);
        FilterReportsCommand = new AsyncRelayCommand(FilterReportsAsync);
        ClearFiltersCommand = new AsyncRelayCommand(ClearFiltersAsync);

        ToggleAdditionalFiltersCommand = new RelayCommandSync(_ =>
        {
            AreAdditionalFiltersVisible = !AreAdditionalFiltersVisible;
        });

        PreviewReportCommand = new RelayCommand<DoctorReportArchiveListItem?>(async r =>
        {
            await PreviewReportAsync(r);
        });

        EditReportCommand = new RelayCommand<DoctorReportArchiveListItem?>(async r =>
        {
            await EditReportAsync(r);
        });

        GeneratePdfCommand = new RelayCommand<DoctorReportArchiveListItem?>(async r =>
        {
            await GeneratePdfAsync(r);
        });

        ArchiveReportCommand = new RelayCommand<DoctorReportArchiveListItem?>(async r =>
        {
            await ArchiveReportAsync(r);
        });

        CloseDetailsCommand = new RelayCommandSync(_ => CloseDetails());

        GoBackCommand = main.GoBackCommand;
    }

    private async Task LoadReportsAsync()
    {
        await LoadTemplatesAsync();
        await FilterReportsAsync();
    }

    private async Task LoadTemplatesAsync()
    {
        var currentSelectedTemplateId = SelectedTemplate?.Id;

        Templates.Clear();

        Templates.Add(new TemplateSummaryDto
        {
            Id = Guid.Empty,
            Name = "Все шаблоны"
        });

        var result = await _templateService.SearchForDoctorAsync(new TemplateSearchRequest
        {
            IncludeDeleted = false
        });

        if (result.IsSuccess && result.Data != null)
        {
            foreach (var template in result.Data.OrderBy(x => x.Name))
                Templates.Add(template);
        }

        SelectedTemplate = Templates.FirstOrDefault(x => x.Id == currentSelectedTemplateId)
                           ?? Templates.FirstOrDefault();
    }

    private async Task FilterReportsAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;
        IsDetailsPanelVisible = false;
        SelectedReport = null;
        SelectedReportImage = null;
        _selectedReportFieldDisplayNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        Reports.Clear();
        OnPropertyChanged(nameof(HasReports));

        var createdFromUtc = CreatedFromDate.HasValue
            ? ToUtcStartOfSelectedDate(CreatedFromDate.Value)
            : (DateTime?)null;

        var createdToUtc = CreatedToDate.HasValue
            ? ToUtcEndOfSelectedDate(CreatedToDate.Value)
            : (DateTime?)null;

        if (createdFromUtc.HasValue && createdToUtc.HasValue && createdFromUtc > createdToUtc)
        {
            ErrorMessage = "Дата начала периода не может быть позже даты окончания.";
            return;
        }

        var selectedTemplateId = AreAdditionalFiltersVisible
                                 && SelectedTemplate != null
                                 && SelectedTemplate.Id != Guid.Empty
            ? SelectedTemplate.Id
            : (Guid?)null;

        var filter = new ReportSearchRequest
        {
            DoctorId = _main.CurrentUser.UserId,
            SearchText = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim(),
            Status = MapNullableReportStatus(SelectedStatusText),
            CreatedFromUtc = createdFromUtc,
            CreatedToUtc = createdToUtc,
            TemplateId = selectedTemplateId,
            IncludeDeleted = IncludeDeleted
        };

        IsBusy = true;

        try
        {
            var result = await _reportService.SearchAsync(filter);

            if (!result.IsSuccess || result.Data == null)
            {
                ErrorMessage = result.ErrorMessage;
                return;
            }

            foreach (var report in result.Data
                         .OrderByDescending(x => x.AppointmentStartAtUtc)
                         .Select(x => new DoctorReportArchiveListItem(x)))
            {
                Reports.Add(report);
            }

            OnPropertyChanged(nameof(HasReports));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ClearFiltersAsync()
    {
        SearchText = string.Empty;
        CreatedFromDate = null;
        CreatedToDate = null;
        SelectedStatusText = StatusOptions[0];
        SelectedTemplate = Templates.FirstOrDefault();
        IncludeDeleted = false;

        await FilterReportsAsync();
    }

    private async Task PreviewReportAsync(DoctorReportArchiveListItem? report)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (report == null)
            return;

        IsBusy = true;

        try
        {
            var result = await _reportService.GetByIdAsync(report.Id);

            if (!result.IsSuccess || result.Data == null)
            {
                ErrorMessage = result.ErrorMessage ?? "Не удалось загрузить отчёт.";
                return;
            }

            var fullReport = result.Data;

            _selectedReportFieldDisplayNames = await LoadFieldDisplayNamesAsync(fullReport.TemplateId);

            SelectedReportImage = BuildReportImage(fullReport);
            SelectedReport = fullReport;

            IsDetailsPanelVisible = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<Dictionary<string, string>> LoadFieldDisplayNamesAsync(Guid templateId)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (templateId == Guid.Empty)
            return result;

        var templateResult = await _templateService.GetByIdAsync(templateId);

        if (!templateResult.IsSuccess || templateResult.Data == null)
            return result;

        foreach (var block in templateResult.Data.Blocks)
        {
            foreach (var field in block.Fields)
            {
                if (string.IsNullOrWhiteSpace(field.FieldName))
                    continue;

                result[field.FieldName.Trim()] = string.IsNullOrWhiteSpace(field.DisplayName)
                    ? field.FieldName.Trim()
                    : field.DisplayName.Trim();
            }
        }

        return result;
    }

    private void CloseDetails()
    {
        SelectedReport = null;
        SelectedReportImage = null;
        IsDetailsPanelVisible = false;
        _selectedReportFieldDisplayNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        OnPropertyChanged(nameof(SelectedReportContentText));
        OnPropertyChanged(nameof(SelectedReportStatusText));
        OnPropertyChanged(nameof(SelectedReportAppointmentTimeText));
        OnPropertyChanged(nameof(SelectedReportImageText));
        OnPropertyChanged(nameof(HasSelectedReportImage));
    }

    private async Task EditReportAsync(DoctorReportArchiveListItem? report)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (report == null)
            return;

        if (!report.CanEdit)
        {
            ErrorMessage = "Редактирование доступно только для черновиков и отчётов в процессе.";
            return;
        }

        var editorVm = new ReportEditorViewModel(
            _main,
            _appointmentService,
            _reportService,
            _templateService,
            _voiceApiService,
            _audioRecorderService);

        await editorVm.LoadReportAsync(report.Id);

        _main.UpdateCurrentView(editorVm);
    }

    private async Task GeneratePdfAsync(DoctorReportArchiveListItem? report)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (report == null)
            return;

        if (!report.CanPrint)
        {
            ErrorMessage = "Печать доступна только для завершённых или архивированных отчётов.";
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _reportService.GetPdfAsync(report.Id);

            if (!result.IsSuccess || result.Data == null)
            {
                ErrorMessage = result.ErrorMessage ?? "Не удалось сформировать PDF.";
                return;
            }

            var filePath = Path.Combine(Path.GetTempPath(), result.Data.FileName);

            await File.WriteAllBytesAsync(filePath, result.Data.Content);

            ReportApiService.OpenFile(filePath);

            SuccessMessage = "PDF сформирован и открыт.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ArchiveReportAsync(DoctorReportArchiveListItem? report)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (report == null)
            return;

        if (!report.CanArchive)
        {
            ErrorMessage = "Архивировать можно только завершённый отчёт.";
            return;
        }

        IsBusy = true;

        try
        {
            var fullResult = await _reportService.GetByIdAsync(report.Id);

            if (!fullResult.IsSuccess || fullResult.Data == null)
            {
                ErrorMessage = fullResult.ErrorMessage ?? "Не удалось загрузить отчёт для архивирования.";
                return;
            }

            var fullReport = fullResult.Data;

            var command = new UpdateReportCommand
            {
                ReportId = fullReport.Id,
                Status = ReportStatus.Archived,
                ContentJson = fullReport.ContentJson,
                ExpectedVersion = fullReport.Version
            };

            var result = await _reportService.UpdateAsync(command);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.ErrorMessage ?? "Не удалось архивировать отчёт.";
                return;
            }

            SuccessMessage = "Отчёт архивирован.";

            CloseDetails();
            await FilterReportsAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static Bitmap? BuildReportImage(ReportDto report)
    {
        if (!report.HasUltrasoundImage)
            return null;

        if (string.IsNullOrWhiteSpace(report.UltrasoundImageBase64))
            return null;

        try
        {
            var bytes = Convert.FromBase64String(report.UltrasoundImageBase64);
            var stream = new MemoryStream(bytes);

            return new Bitmap(stream);
        }
        catch
        {
            return null;
        }
    }

    private static string FormatContentJson(
        string? contentJson,
        IReadOnlyDictionary<string, string> fieldDisplayNames)
    {
        if (string.IsNullOrWhiteSpace(contentJson) || contentJson == "{}")
            return "Содержимое отчёта пока не заполнено.";

        try
        {
            using var document = JsonDocument.Parse(contentJson);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
                return contentJson;

            var lines = new List<string>();

            foreach (var property in root.EnumerateObject())
            {
                var value = property.Value.ValueKind == JsonValueKind.String
                    ? property.Value.GetString()
                    : property.Value.GetRawText();

                if (string.IsNullOrWhiteSpace(value))
                    continue;

                var displayName = GetDisplayFieldName(property.Name, fieldDisplayNames);

                lines.Add($"{displayName}: {value}");
            }

            return lines.Count == 0
                ? "Содержимое отчёта пока не заполнено."
                : string.Join(Environment.NewLine, lines);
        }
        catch
        {
            return contentJson;
        }
    }

    private static string GetDisplayFieldName(
        string fieldName,
        IReadOnlyDictionary<string, string> fieldDisplayNames)
    {
        if (fieldDisplayNames.TryGetValue(fieldName, out var displayName) &&
            !string.IsNullOrWhiteSpace(displayName))
        {
            return displayName;
        }

        return fieldName.Trim().ToLowerInvariant() switch
        {
            "description" => "Описание",
            "conclusion" => "Заключение",
            "comment" => "Комментарий",
            "comments" => "Комментарии",
            "result" => "Результат",
            "recommendation" => "Рекомендация",
            "recommendations" => "Рекомендации",
            _ => fieldName
        };
    }

    private static ReportStatus? MapNullableReportStatus(string? statusText)
    {
        return statusText switch
        {
            "Черновик" => ReportStatus.Draft,
            "В процессе" => ReportStatus.InProgress,
            "Завершён" => ReportStatus.Completed,
            "Архивирован" => ReportStatus.Archived,
            _ => null
        };
    }

    private static string GetReportStatusText(ReportStatus status)
    {
        return status switch
        {
            ReportStatus.Draft => "Черновик",
            ReportStatus.InProgress => "В процессе",
            ReportStatus.Completed => "Завершён",
            ReportStatus.Archived => "Архивирован",
            _ => status.ToString()
        };
    }

    private static DateTime ToUtcStartOfSelectedDate(DateTimeOffset selectedDate)
    {
        var date = selectedDate.Date;
        return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
    }

    private static DateTime ToUtcEndOfSelectedDate(DateTimeOffset selectedDate)
    {
        var date = selectedDate.Date;
        return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, DateTimeKind.Utc);
    }

    private static DateTime ToLocalTime(DateTime value)
    {
        if (value == DateTime.MinValue)
            return value;

        return value.Kind switch
        {
            DateTimeKind.Utc => value.ToLocalTime(),
            DateTimeKind.Local => value,
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc).ToLocalTime()
        };
    }
}
