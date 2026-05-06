using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;
using UltrasoundAssistant.DoctorClient.Helpers;
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
    private readonly VoiceApiService _voiceApiService;
    private readonly IAudioRecorderService _audioRecorderService;

    public ObservableCollection<ReportSummaryDto> Reports { get; } = new();
    public ObservableCollection<ReportStatus?> Statuses { get; } = new();
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

    private ReportStatus? _selectedStatus;
    public ReportStatus? SelectedStatus
    {
        get => _selectedStatus;
        set => SetProperty(ref _selectedStatus, value);
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
                OnPropertyChanged(nameof(SelectedReportContentText));
        }
    }

    private bool _isDetailsPanelVisible;
    public bool IsDetailsPanelVisible
    {
        get => _isDetailsPanelVisible;
        set => SetProperty(ref _isDetailsPanelVisible, value);
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

    public string SelectedReportContentText =>
        SelectedReport == null
            ? string.Empty
            : FormatContentJson(SelectedReport.ContentJson);

    public ICommand LoadReportsCommand { get; }
    public ICommand FilterReportsCommand { get; }
    public ICommand ClearFiltersCommand { get; }
    public ICommand ToggleAdditionalFiltersCommand { get; }
    public ICommand PreviewReportCommand { get; }
    public ICommand EditReportCommand { get; }
    public ICommand GeneratePdfCommand { get; }
    public ICommand CloseDetailsCommand { get; }
    public ICommand GoBackCommand { get; }

    public DoctorReportsArchiveViewModel(
        MainWindowViewModel main,
        ReportApiService reportApiService,
        TemplateApiService templateApiService,
        VoiceApiService voiceApiService,
        IAudioRecorderService audioRecorderService)
    {
        _main = main;
        _reportService = reportApiService;
        _templateService = templateApiService;
        _voiceApiService = voiceApiService;
        _audioRecorderService = audioRecorderService;

        Statuses.Add(null);
        foreach (var status in Enum.GetValues<ReportStatus>())
            Statuses.Add(status);

        LoadReportsCommand = new AsyncRelayCommand(LoadReportsAsync);
        FilterReportsCommand = new AsyncRelayCommand(FilterReportsAsync);
        ClearFiltersCommand = new AsyncRelayCommand(ClearFiltersAsync);

        ToggleAdditionalFiltersCommand = new RelayCommandSync(_ =>
        {
            AreAdditionalFiltersVisible = !AreAdditionalFiltersVisible;
        });

        PreviewReportCommand = new RelayCommand<ReportSummaryDto?>(async r =>
        {
            await PreviewReportAsync(r);
        });

        EditReportCommand = new RelayCommand<ReportSummaryDto?>(async r =>
        {
            await EditReportAsync(r);
        });

        GeneratePdfCommand = new RelayCommand<ReportSummaryDto?>(async r =>
        {
            await GeneratePdfAsync(r);
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
        var result = await _templateService.SearchForDoctorAsync(new TemplateSearchRequest
        {
            IncludeDeleted = false
        });

        Templates.Clear();

        if (result.IsSuccess && result.Data != null)
        {
            foreach (var template in result.Data.OrderBy(x => x.Name))
                Templates.Add(template);
        }
    }

    private async Task FilterReportsAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;
        IsDetailsPanelVisible = false;
        SelectedReport = null;
        Reports.Clear();

        var filter = new ReportSearchRequest
        {
            DoctorId = _main.CurrentUser.UserId,
            SearchText = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim(),
            Status = SelectedStatus,
            CreatedFromUtc = CreatedFromDate?.DateTime.Date.ToUniversalTime(),
            CreatedToUtc = CreatedToDate?.DateTime.Date.AddDays(1).ToUniversalTime(),
            TemplateId = AreAdditionalFiltersVisible ? SelectedTemplate?.Id : null,
            IncludeDeleted = IncludeDeleted
        };

        var result = await _reportService.SearchAsync(filter);

        if (!result.IsSuccess || result.Data == null)
        {
            ErrorMessage = result.ErrorMessage;
            return;
        }

        foreach (var report in result.Data.OrderByDescending(x => x.AppointmentStartAtUtc))
            Reports.Add(report);
    }

    private async Task ClearFiltersAsync()
    {
        SearchText = string.Empty;
        CreatedFromDate = null;
        CreatedToDate = null;
        SelectedStatus = null;
        SelectedTemplate = null;
        IncludeDeleted = false;

        await FilterReportsAsync();
    }

    private async Task PreviewReportAsync(ReportSummaryDto? report)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (report == null)
            return;

        var result = await _reportService.GetByIdAsync(report.Id);

        if (!result.IsSuccess || result.Data == null)
        {
            ErrorMessage = result.ErrorMessage ?? "Не удалось загрузить отчёт.";
            return;
        }

        SelectedReport = result.Data;
        IsDetailsPanelVisible = true;
    }

    private void CloseDetails()
    {
        SelectedReport = null;
        IsDetailsPanelVisible = false;
        OnPropertyChanged(nameof(SelectedReportContentText));
    }

    private async Task EditReportAsync(ReportSummaryDto? report)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (report == null)
            return;

        if (report.Status is ReportStatus.Completed or ReportStatus.Archived)
        {
            ErrorMessage = "Завершённые и архивированные отчёты нельзя редактировать.";
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

    private async Task GeneratePdfAsync(ReportSummaryDto? report)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (report == null)
            return;

        if (report.Status is not (ReportStatus.Completed or ReportStatus.Archived))
        {
            ErrorMessage = "Печать доступна только для завершённых или архивированных отчётов.";
            return;
        }

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

    private static string FormatContentJson(string? contentJson)
    {
        if (string.IsNullOrWhiteSpace(contentJson) || contentJson == "{}")
            return string.Empty;

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

                lines.Add($"{property.Name}: {value}");
            }

            return string.Join(Environment.NewLine, lines);
        }
        catch
        {
            return contentJson;
        }
    }
}
