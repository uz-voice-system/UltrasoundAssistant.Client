using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;
using System.Windows.Input;
using UltrasoundAssistant.DoctorClient.Models.Commands.Appointments;
using UltrasoundAssistant.DoctorClient.Models.Commands.Reports;
using UltrasoundAssistant.DoctorClient.Models.Entity.Templates;
using UltrasoundAssistant.DoctorClient.Models.Enums;
using UltrasoundAssistant.DoctorClient.Models.Reads.Appointments.Details;
using UltrasoundAssistant.DoctorClient.Models.Reads.Reports.Details;
using UltrasoundAssistant.DoctorClient.Models.Reads.Templates.Details;
using UltrasoundAssistant.DoctorClient.Models.Voice;
using UltrasoundAssistant.DoctorClient.Services;
using UltrasoundAssistant.DoctorClient.Services.AudioService;

namespace UltrasoundAssistant.DoctorClient.ViewModels.Report;

public partial class ReportEditorViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _main;
    private readonly ReportApiService _reportService;
    private readonly TemplateApiService _templateService;
    private readonly VoiceApiService _voiceApiService;
    private readonly IAudioRecorderService _audioRecorderService;
    private readonly AppointmentApiService _appointmentService;
    
    private Guid _reportId;
    private Guid _templateId;
    private Guid _appointmentId;
    private int _reportVersion;

    public ObservableCollection<ReportFieldEditorItem> ReportFields { get; } = new();
    public ObservableCollection<ReportBlockEditorItem> ReportBlocks { get; } = new();

    [ObservableProperty]
    private string patientName = string.Empty;

    [ObservableProperty]
    private string doctorName = string.Empty;

    [ObservableProperty]
    private string templateName = string.Empty;

    [ObservableProperty]
    private string appointmentTimeText = string.Empty;

    [ObservableProperty]
    private ReportStatus status = ReportStatus.Draft;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isRecording;

    [ObservableProperty]
    private bool isRecognizing;

    [ObservableProperty]
    private string recognizedText = string.Empty;

    [ObservableProperty]
    private string unmatchedRecognitions = string.Empty;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private string? successMessage;

    [ObservableProperty]
    private bool hasUltrasoundImage;

    [ObservableProperty]
    private string ultrasoundImageFileName = string.Empty;

    [ObservableProperty]
    private string ultrasoundImageUploadedText = string.Empty;

    [ObservableProperty]
    private Bitmap? ultrasoundImagePreview;

    public string StatusText => Status switch
    {
        ReportStatus.Draft => "Черновик",
        ReportStatus.InProgress => "В процессе",
        ReportStatus.Completed => "Завершён",
        ReportStatus.Archived => "Архивирован",
        _ => Status.ToString()
    };

    public string UltrasoundImageInfoText
    {
        get
        {
            if (!HasUltrasoundImage)
                return "Изображение УЗИ не загружено.";

            if (string.IsNullOrWhiteSpace(UltrasoundImageUploadedText))
                return string.IsNullOrWhiteSpace(UltrasoundImageFileName)
                    ? "Изображение УЗИ загружено."
                    : $"Загружено: {UltrasoundImageFileName}";

            return string.IsNullOrWhiteSpace(UltrasoundImageFileName)
                ? UltrasoundImageUploadedText
                : $"Загружено: {UltrasoundImageFileName}. {UltrasoundImageUploadedText}";
        }
    }

    public bool IsCompletedOrArchived => Status is ReportStatus.Completed or ReportStatus.Archived;

    public bool CanEditReportFields => !IsBusy && !IsRecognizing && !IsCompletedOrArchived;
    public bool CanStartRecording => !IsBusy && !IsRecognizing && !IsRecording && !IsCompletedOrArchived;
    public bool CanStopRecording => !IsBusy && !IsRecognizing && IsRecording && !IsCompletedOrArchived;
    public bool CanSave => !IsBusy && !IsRecognizing && !IsCompletedOrArchived;
    public bool CanComplete => !IsBusy && !IsRecognizing && !IsCompletedOrArchived;
    public bool CanGeneratePdf => _reportId != Guid.Empty && IsCompletedOrArchived && !IsBusy && !IsRecognizing;

    public bool CanUploadImage =>
        _reportId != Guid.Empty &&
        !IsBusy &&
        !IsRecognizing &&
        !IsCompletedOrArchived;

    public bool CanDeleteImage =>
        _reportId != Guid.Empty &&
        HasUltrasoundImage &&
        !IsBusy &&
        !IsRecognizing &&
        !IsCompletedOrArchived;

    public ICommand StartRecordingCommand { get; }
    public ICommand StopRecordingCommand { get; }
    public ICommand SaveReportCommand { get; }
    public ICommand CompleteReportCommand { get; }
    public ICommand GeneratePdfCommand { get; }
    public ICommand DeleteImageCommand { get; }
    public ICommand GoBackCommand { get; }

    public ReportEditorViewModel(
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

        StartRecordingCommand = new AsyncRelayCommand(StartRecordingAsync);
        StopRecordingCommand = new AsyncRelayCommand(StopRecordingAsync);
        SaveReportCommand = new AsyncRelayCommand(SaveReportAsync);
        CompleteReportCommand = new AsyncRelayCommand(CompleteReportAsync);
        GeneratePdfCommand = new AsyncRelayCommand(GeneratePdfAsync);
        DeleteImageCommand = new AsyncRelayCommand(DeleteImageAsync);
        GoBackCommand = main.GoBackCommand;
    }

    partial void OnIsBusyChanged(bool value) => RaiseStateProperties();
    partial void OnIsRecordingChanged(bool value) => RaiseStateProperties();
    partial void OnIsRecognizingChanged(bool value) => RaiseStateProperties();

    partial void OnStatusChanged(ReportStatus value)
    {
        RaiseStateProperties();
        OnPropertyChanged(nameof(StatusText));
    }

    partial void OnHasUltrasoundImageChanged(bool value)
    {
        OnPropertyChanged(nameof(UltrasoundImageInfoText));
        RaiseStateProperties();
    }

    partial void OnUltrasoundImageFileNameChanged(string value)
    {
        OnPropertyChanged(nameof(UltrasoundImageInfoText));
    }

    partial void OnUltrasoundImageUploadedTextChanged(string value)
    {
        OnPropertyChanged(nameof(UltrasoundImageInfoText));
    }

    private void RaiseStateProperties()
    {
        OnPropertyChanged(nameof(IsCompletedOrArchived));
        OnPropertyChanged(nameof(CanEditReportFields));
        OnPropertyChanged(nameof(CanStartRecording));
        OnPropertyChanged(nameof(CanStopRecording));
        OnPropertyChanged(nameof(CanSave));
        OnPropertyChanged(nameof(CanComplete));
        OnPropertyChanged(nameof(CanGeneratePdf));
        OnPropertyChanged(nameof(CanUploadImage));
        OnPropertyChanged(nameof(CanDeleteImage));
    }

    public async Task LoadReportAsync(Guid reportId)
    {
        ErrorMessage = null;
        SuccessMessage = null;
        IsBusy = true;

        try
        {
            var reportResult = await _reportService.GetByIdAsync(reportId);

            if (!reportResult.IsSuccess || reportResult.Data == null)
            {
                ErrorMessage = reportResult.ErrorMessage ?? "Не удалось загрузить отчёт.";
                return;
            }

            var report = reportResult.Data;

            _reportId = report.Id;
            _appointmentId = report.AppointmentId;
            _templateId = report.TemplateId;
            _reportVersion = report.Version;

            PatientName = report.PatientFullName;
            DoctorName = report.DoctorFullName;
            TemplateName = report.TemplateName;
            Status = report.Status;
            AppointmentTimeText = FormatAppointmentPeriod(report.AppointmentStartAtUtc, report.AppointmentEndAtUtc);

            ApplyImageState(report);

            var templateResult = await _templateService.GetByIdAsync(report.TemplateId);

            if (!templateResult.IsSuccess || templateResult.Data == null)
            {
                ErrorMessage = templateResult.ErrorMessage ?? "Не удалось загрузить шаблон.";
                return;
            }

            BuildFields(templateResult.Data, report);
            RecognizedText = string.Empty;
            UnmatchedRecognitions = string.Empty;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyImageState(ReportDto report)
    {
        HasUltrasoundImage = report.HasUltrasoundImage;
        UltrasoundImageFileName = report.UltrasoundImageFileName ?? string.Empty;

        UltrasoundImageUploadedText = report.UltrasoundImageUploadedAtUtc == null
            ? string.Empty
            : $"Дата загрузки: {FormatDateTimeLocal(report.UltrasoundImageUploadedAtUtc.Value)}";

        UltrasoundImagePreview = TryCreateBitmapFromBase64(report.UltrasoundImageBase64);
    }

    private void BuildFields(TemplateDto template, ReportDto report)
    {
        ReportFields.Clear();
        ReportBlocks.Clear();

        var values = ParseContentJson(report.ContentJson);
        var usedFieldNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var block in template.Blocks.OrderBy(x => x.Position))
        {
            var blockItem = new ReportBlockEditorItem
            {
                BlockName = block.Name,
                BlockPhrasesText = block.Phrases.Count == 0
                    ? "Фразы блока не заданы"
                    : string.Join(", ", block.Phrases)
            };

            foreach (var field in block.Fields.OrderBy(x => x.Position))
            {
                values.TryGetValue(field.FieldName, out var value);

                var fieldItem = new ReportFieldEditorItem
                {
                    BlockName = block.Name,
                    FieldName = field.FieldName,
                    DisplayName = field.DisplayName,
                    Role = field.Role,
                    PhrasesText = field.Phrases.Count == 0
                        ? field.DisplayName
                        : string.Join(", ", field.Phrases),
                    Value = value ?? string.Empty,
                    OriginalValue = value ?? string.Empty,
                    NormMessage = BuildNormText(field.Norm)
                };

                fieldItem.ClearRecognition();

                blockItem.Fields.Add(fieldItem);
                ReportFields.Add(fieldItem);

                usedFieldNames.Add(field.FieldName);
            }

            ReportBlocks.Add(blockItem);
        }

        var extraValues = values
            .Where(x => !usedFieldNames.Contains(x.Key))
            .OrderBy(x => x.Key)
            .ToList();

        if (extraValues.Count == 0)
            return;

        var extraBlock = new ReportBlockEditorItem
        {
            BlockName = "Дополнительно",
            BlockPhrasesText = "Поля, которых нет в текущем шаблоне"
        };

        foreach (var extra in extraValues)
        {
            var fieldItem = new ReportFieldEditorItem
            {
                BlockName = "Дополнительно",
                FieldName = extra.Key,
                DisplayName = extra.Key,
                Role = TemplateFieldRole.Regular,
                PhrasesText = extra.Key,
                Value = extra.Value,
                OriginalValue = extra.Value
            };

            extraBlock.Fields.Add(fieldItem);
            ReportFields.Add(fieldItem);
        }

        ReportBlocks.Add(extraBlock);
    }

    private static string? BuildNormText(FieldNormDto? norm)
    {
        if (norm == null)
            return null;

        if (!string.IsNullOrWhiteSpace(norm.NormalText))
            return $"Норма: {norm.NormalText}";

        if (norm.Min == null && norm.Max == null)
            return string.IsNullOrWhiteSpace(norm.Unit) ? null : $"Ед. изм.: {norm.Unit}";

        var min = norm.Min.HasValue ? FormatDecimal(norm.Min.Value) : "—";
        var max = norm.Max.HasValue ? FormatDecimal(norm.Max.Value) : "—";
        var unit = string.IsNullOrWhiteSpace(norm.Unit) ? string.Empty : $" {norm.Unit}";

        return $"Норма: {min}–{max}{unit}";
    }

    private static string FormatDecimal(decimal value)
    {
        return value.ToString("0.##", CultureInfo.CurrentCulture);
    }

    private async Task StartRecordingAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (_reportId == Guid.Empty)
        {
            ErrorMessage = "Отчёт не загружен.";
            return;
        }

        if (IsCompletedOrArchived)
        {
            ErrorMessage = "Завершённый или архивированный отчёт нельзя редактировать.";
            return;
        }

        IsBusy = true;

        try
        {
            await _audioRecorderService.StartAsync();

            IsRecording = true;
            SuccessMessage = "Запись начата.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task StopRecordingAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (!IsRecording)
        {
            ErrorMessage = "Запись не запущена.";
            return;
        }

        IsBusy = true;

        try
        {
            var wavBytes = await _audioRecorderService.StopAsync();
            IsRecording = false;

            if (wavBytes == null || wavBytes.Length == 0)
            {
                ErrorMessage = "Аудио не записано.";
                return;
            }

            IsRecognizing = true;

            var request = new VoiceProcessRequest
            {
                ReportId = _reportId,
                TemplateId = _templateId,
                AudioBase64 = Convert.ToBase64String(wavBytes),
                Language = "ru",
                FileName = $"report_{_reportId:N}.wav"
            };

            var result = await _voiceApiService.ProcessAsync(request);

            if (!result.IsSuccess || result.Data == null)
            {
                ErrorMessage = result.ErrorMessage ?? "Не удалось обработать голосовой ввод.";
                return;
            }

            if (!string.IsNullOrWhiteSpace(result.Data.Error))
            {
                ErrorMessage = result.Data.Error;
                return;
            }

            ApplyVoiceResult(result.Data);

            RecognizedText = string.Join(
                Environment.NewLine,
                result.Data.Fields.Select(x => $"{x.BlockName} / {x.Keyword}: {x.Value}"));

            SuccessMessage = "Распознавание завершено.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            IsRecording = false;
        }
        finally
        {
            IsRecognizing = false;
            IsBusy = false;
        }
    }

    private void ApplyVoiceResult(VoiceProcessResult result)
    {
        foreach (var matched in result.Fields)
        {
            var field = ReportFields.FirstOrDefault(x =>
                string.Equals(x.FieldName, matched.FieldName, StringComparison.OrdinalIgnoreCase));

            if (field == null)
            {
                AppendUnmatched($"[{matched.BlockName} / {matched.Keyword}] {matched.Value}");
                continue;
            }

            field.Value = matched.Value ?? string.Empty;
            field.WasRecognized = true;
            field.Confidence = matched.Confidence;
            field.NormStatus = matched.NormStatus;

            if (!string.IsNullOrWhiteSpace(matched.NormMessage))
                field.NormMessage = matched.NormMessage;
        }

        foreach (var unmatched in result.UnmatchedParts)
            AppendUnmatched(unmatched);
    }

    private void AppendUnmatched(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        UnmatchedRecognitions = string.IsNullOrWhiteSpace(UnmatchedRecognitions)
            ? text.Trim()
            : $"{UnmatchedRecognitions}{Environment.NewLine}{text.Trim()}";
    }

    private async Task SaveReportAsync()
    {
        await SaveReportWithStatusAsync(ReportStatus.InProgress, "Отчёт сохранён. Статус: в процессе.");
    }

    private async Task CompleteReportAsync()
    {
        await SaveReportWithStatusAsync(ReportStatus.Completed, "Отчёт завершён. Запись на приём завершена.");
    }

    private async Task SaveReportWithStatusAsync(ReportStatus newStatus, string successMessage)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (_reportId == Guid.Empty)
        {
            ErrorMessage = "Отчёт не загружен.";
            return;
        }

        if (IsCompletedOrArchived)
        {
            ErrorMessage = "Завершённый или архивированный отчёт нельзя редактировать.";
            return;
        }

        IsBusy = true;

        try
        {
            var command = new UpdateReportCommand
            {
                ReportId = _reportId,
                Status = newStatus,
                ContentJson = BuildContentJson(),
                ExpectedVersion = _reportVersion
            };

            var result = await _reportService.UpdateAsync(command);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.ErrorMessage;
                return;
            }

            _reportVersion++;
            Status = newStatus;

            foreach (var field in ReportFields)
                field.OriginalValue = field.Value ?? string.Empty;

            var targetAppointmentStatus = newStatus == ReportStatus.Completed
                ? AppointmentStatus.Completed
                : AppointmentStatus.InProgress;

            var appointmentStatusUpdated = await UpdateAppointmentStatusAsync(targetAppointmentStatus);

            if (!appointmentStatusUpdated)
                return;

            SuccessMessage = successMessage;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<bool> UpdateAppointmentStatusAsync(AppointmentStatus targetStatus)
    {
        if (_appointmentId == Guid.Empty)
            return true;

        var appointmentResult = await _appointmentService.GetByIdAsync(_appointmentId);

        if (!appointmentResult.IsSuccess || appointmentResult.Data == null)
        {
            ErrorMessage = appointmentResult.ErrorMessage
                           ?? "Отчёт сохранён, но не удалось загрузить запись на приём для обновления статуса.";
            return false;
        }

        var appointment = appointmentResult.Data;

        if (appointment.Status == targetStatus)
            return true;

        var command = BuildUpdateAppointmentStatusCommand(appointment, targetStatus);

        var result = await _appointmentService.UpdateAsync(command);

        if (!result.IsSuccess)
        {
            ErrorMessage = result.ErrorMessage
                           ?? "Отчёт сохранён, но не удалось обновить статус записи на приём.";
            return false;
        }

        return true;
    }

    private static UpdateAppointmentCommand BuildUpdateAppointmentStatusCommand(AppointmentDto appointment, AppointmentStatus targetStatus)
    {
        return new UpdateAppointmentCommand
        {
            AppointmentId = appointment.Id,
            PatientId = appointment.PatientId,
            DoctorId = appointment.DoctorId,
            TemplateId = appointment.TemplateId,
            StartAtUtc = appointment.StartAtUtc,
            EndAtUtc = appointment.EndAtUtc,
            Status = targetStatus,
            Comment = appointment.Comment,
            ExpectedVersion = appointment.Version
        };
    }

    public async Task UploadImageFromPathAsync(string? filePath)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (string.IsNullOrWhiteSpace(filePath))
        {
            ErrorMessage = "Файл изображения не выбран.";
            return;
        }

        if (_reportId == Guid.Empty)
        {
            ErrorMessage = "Отчёт не загружен.";
            return;
        }

        if (IsCompletedOrArchived)
        {
            ErrorMessage = "Завершённый или архивированный отчёт нельзя редактировать.";
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _reportService.UploadImageAsync(
                _reportId,
                filePath,
                _reportVersion);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.ErrorMessage ?? "Не удалось загрузить изображение.";
                return;
            }

            _reportVersion++;

            HasUltrasoundImage = true;
            UltrasoundImageFileName = Path.GetFileName(filePath);
            UltrasoundImageUploadedText = $"Дата загрузки: {DateTime.Now:dd.MM.yyyy HH:mm}";
            UltrasoundImagePreview = TryCreateBitmapFromFile(filePath);

            SuccessMessage = "Изображение УЗИ загружено.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteImageAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (_reportId == Guid.Empty)
        {
            ErrorMessage = "Отчёт не загружен.";
            return;
        }

        if (!HasUltrasoundImage)
        {
            ErrorMessage = "У отчёта нет загруженного изображения.";
            return;
        }

        if (IsCompletedOrArchived)
        {
            ErrorMessage = "Завершённый или архивированный отчёт нельзя редактировать.";
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _reportService.DeleteImageAsync(_reportId, _reportVersion);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.ErrorMessage ?? "Не удалось удалить изображение.";
                return;
            }

            _reportVersion++;

            HasUltrasoundImage = false;
            UltrasoundImageFileName = string.Empty;
            UltrasoundImageUploadedText = string.Empty;
            UltrasoundImagePreview = null;

            SuccessMessage = "Изображение УЗИ удалено.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task GeneratePdfAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (_reportId == Guid.Empty)
        {
            ErrorMessage = "Отчёт не загружен.";
            return;
        }

        if (!IsCompletedOrArchived)
        {
            ErrorMessage = "Печать доступна только для завершённого или архивированного отчёта.";
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _reportService.GetPdfAsync(_reportId);

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

    private string BuildContentJson()
    {
        var values = ReportFields
            .Where(x => !string.IsNullOrWhiteSpace(x.FieldName))
            .GroupBy(x => x.FieldName.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                x => x.Key,
                x => x.Last().Value?.Trim() ?? string.Empty,
                StringComparer.OrdinalIgnoreCase);

        return JsonSerializer.Serialize(values);
    }

    private static Dictionary<string, string> ParseContentJson(string? contentJson)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(contentJson) || contentJson == "{}")
            return result;

        try
        {
            using var document = JsonDocument.Parse(contentJson);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
                return result;

            foreach (var property in root.EnumerateObject())
            {
                var value = property.Value.ValueKind == JsonValueKind.String
                    ? property.Value.GetString()
                    : property.Value.GetRawText();

                result[property.Name] = value ?? string.Empty;
            }
        }
        catch
        {
            return result;
        }

        return result;
    }

    private static string FormatAppointmentPeriod(DateTime? startUtc, DateTime? endUtc)
    {
        if (startUtc == null)
            return string.Empty;

        var startLocal = EnsureUtc(startUtc.Value).ToLocalTime();

        if (endUtc == null)
            return $"{startLocal:dd.MM.yyyy HH:mm}";

        var endLocal = EnsureUtc(endUtc.Value).ToLocalTime();

        return $"{startLocal:dd.MM.yyyy HH:mm}–{endLocal:HH:mm}";
    }

    private static string FormatDateTimeLocal(DateTime value)
    {
        return $"{EnsureUtc(value).ToLocalTime():dd.MM.yyyy HH:mm}";
    }

    private static DateTime EnsureUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    private static Bitmap? TryCreateBitmapFromBase64(string? base64)
    {
        if (string.IsNullOrWhiteSpace(base64))
            return null;

        try
        {
            var bytes = Convert.FromBase64String(base64);
            using var stream = new MemoryStream(bytes);
            return new Bitmap(stream);
        }
        catch
        {
            return null;
        }
    }

    private static Bitmap? TryCreateBitmapFromFile(string filePath)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            return new Bitmap(stream);
        }
        catch
        {
            return null;
        }
    }
}
