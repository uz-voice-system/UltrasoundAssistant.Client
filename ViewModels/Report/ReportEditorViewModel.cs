using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;
using UltrasoundAssistant.DoctorClient.Models.Commands.Report;
using UltrasoundAssistant.DoctorClient.Models.Read.Report;
using UltrasoundAssistant.DoctorClient.Models.Read.Template;
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

    private Guid _reportId;
    private int _reportVersion;
    private Guid _templateId;

    public ObservableCollection<ReportFieldEditorItem> ReportFields { get; } = new();

    [ObservableProperty]
    private string patientName = string.Empty;

    [ObservableProperty]
    private string templateName = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isRecording;

    [ObservableProperty]
    private bool isPaused;

    [ObservableProperty]
    private bool isCompleted;

    [ObservableProperty]
    private string recognizedText = string.Empty;

    [ObservableProperty]
    private string unmatchedRecognitions = string.Empty;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private string? successMessage;

    [ObservableProperty]
    private bool isRecognizing;

    public bool CanEditReportFields => !IsBusy && !IsCompleted && !IsRecognizing;
    public bool CanStartRecording => !IsBusy && !IsCompleted && !IsRecognizing && (!IsRecording || IsPaused);
    public bool CanPauseRecording => !IsBusy && !IsCompleted && !IsRecognizing && IsRecording && !IsPaused;
    public bool CanStopRecording => !IsBusy && !IsCompleted && !IsRecognizing && IsRecording;
    public bool CanSave => !IsBusy && !IsCompleted && !IsRecognizing;
    public bool CanComplete => !IsBusy && !IsCompleted && !IsRecognizing;
    public bool CanGeneratePdf => _reportId != Guid.Empty && IsCompleted && !IsBusy && !IsRecognizing;

    public ICommand LoadReportCommand { get; }
    public ICommand StartRecordingCommand { get; }
    public ICommand PauseRecordingCommand { get; }
    public ICommand StopRecordingCommand { get; }
    public ICommand SaveReportCommand { get; }
    public ICommand CompleteReportCommand { get; }
    public ICommand GeneratePdfCommand { get; }
    public ICommand GoBackCommand { get; }

    public ReportEditorViewModel(
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

        LoadReportCommand = new AsyncRelayCommand<Guid>(LoadReportAsync);
        StartRecordingCommand = new AsyncRelayCommand(StartRecordingAsync);
        PauseRecordingCommand = new AsyncRelayCommand(PauseRecordingAsync);
        StopRecordingCommand = new AsyncRelayCommand(StopRecordingAsync);
        SaveReportCommand = new AsyncRelayCommand(SaveReportAsync);
        CompleteReportCommand = new AsyncRelayCommand(CompleteReportAsync);
        GeneratePdfCommand = new AsyncRelayCommand(GeneratePdfAsync);
        GoBackCommand = main.GoBackCommand;
    }

    partial void OnIsBusyChanged(bool value) => RaiseStateProperties();
    partial void OnIsRecordingChanged(bool value) => RaiseStateProperties();
    partial void OnIsPausedChanged(bool value) => RaiseStateProperties();
    partial void OnIsCompletedChanged(bool value) => RaiseStateProperties();
    partial void OnIsRecognizingChanged(bool value) => RaiseStateProperties();

    private void RaiseStateProperties()
    {
        OnPropertyChanged(nameof(CanEditReportFields));
        OnPropertyChanged(nameof(CanStartRecording));
        OnPropertyChanged(nameof(CanPauseRecording));
        OnPropertyChanged(nameof(CanStopRecording));
        OnPropertyChanged(nameof(CanSave));
        OnPropertyChanged(nameof(CanComplete));
        OnPropertyChanged(nameof(CanGeneratePdf));
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
            _reportVersion = report.Version;
            _templateId = report.TemplateId;

            PatientName = report.PatientName ?? string.Empty;
            TemplateName = report.TemplateName ?? string.Empty;
            IsCompleted = report.Status.ToString().Equals("Completed", StringComparison.OrdinalIgnoreCase);

            var templateResult = await _templateService.GetByIdAsync(report.TemplateId);
            if (!templateResult.IsSuccess || templateResult.Data == null)
            {
                ErrorMessage = templateResult.ErrorMessage ?? "Не удалось загрузить шаблон отчёта.";
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

    private void BuildFields(TemplateDto template, ReportDto report)
    {
        ReportFields.Clear();

        var grouped = template.Keywords
            .Where(k => !string.IsNullOrWhiteSpace(k.TargetField))
            .GroupBy(k => k.TargetField.Trim(), StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var group in grouped)
        {
            var fieldName = group.Key;
            var keyword = group.FirstOrDefault()?.Phrase?.Trim() ?? fieldName;

            report.Content.TryGetValue(fieldName, out var value);

            ReportFields.Add(new ReportFieldEditorItem
            {
                FieldName = fieldName,
                Keyword = keyword,
                Value = value ?? string.Empty,
                OriginalValue = value ?? string.Empty
            });
        }

        foreach (var extraField in report.Content.Keys
                     .Where(k => grouped.All(g => !string.Equals(g.Key, k, StringComparison.OrdinalIgnoreCase)))
                     .OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            ReportFields.Add(new ReportFieldEditorItem
            {
                FieldName = extraField,
                Keyword = extraField,
                Value = report.Content[extraField],
                OriginalValue = report.Content[extraField]
            });
        }
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

        IsBusy = true;

        try
        {
            if (IsRecording && IsPaused)
            {
                await _audioRecorderService.ResumeAsync();
                IsPaused = false;
                SuccessMessage = "Запись продолжена.";
            }
            else
            {
                await _audioRecorderService.StartAsync();
                IsRecording = true;
                IsPaused = false;
                SuccessMessage = "Запись начата.";
            }
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

    private async Task PauseRecordingAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (!IsRecording || IsPaused)
            return;

        IsBusy = true;

        try
        {
            await _audioRecorderService.PauseAsync();
            IsPaused = true;
            SuccessMessage = "Запись приостановлена.";
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
            IsPaused = false;

            if (wavBytes == null || wavBytes.Length == 0)
            {
                ErrorMessage = "Аудио не записано.";
                return;
            }

            IsRecognizing = true;

            var voiceRequest = new VoiceProcessRequest
            {
                ReportId = _reportId,
                TemplateId = _templateId,
                AudioBase64 = Convert.ToBase64String(wavBytes),
                Language = "ru",
                FileName = $"report_{_reportId}.wav"
            };

            var voiceResult = await _voiceApiService.ProcessAsync(voiceRequest);

            if (!voiceResult.IsSuccess || voiceResult.Data == null)
            {
                ErrorMessage = voiceResult.ErrorMessage ?? "Не удалось обработать голосовой ввод.";
                return;
            }

            RecognizedText = string.Join(
                Environment.NewLine,
                voiceResult.Data.Fields.Select(f => $"{f.Keyword}: {f.Value}"));

            ApplyVoiceResult(voiceResult.Data);
            SuccessMessage = "Распознавание обработано.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            IsRecording = false;
            IsPaused = false;
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
            var field = ReportFields.FirstOrDefault(f =>
                string.Equals(f.FieldName, matched.FieldName, StringComparison.OrdinalIgnoreCase));

            if (field != null)
            {
                field.Value = matched.Value ?? string.Empty;
            }
            else
            {
                AppendToUnmatched($"[{matched.Keyword}] {matched.Value}");
            }
        }

        foreach (var unmatched in result.UnmatchedParts)
        {
            if (!string.IsNullOrWhiteSpace(unmatched))
                AppendToUnmatched(unmatched);
        }
    }

    private void AppendToUnmatched(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        if (string.IsNullOrWhiteSpace(UnmatchedRecognitions))
            UnmatchedRecognitions = text;
        else
            UnmatchedRecognitions = $"{UnmatchedRecognitions}{Environment.NewLine}{text}";
    }

    private async Task SaveReportAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (_reportId == Guid.Empty)
        {
            ErrorMessage = "Отчёт не загружен.";
            return;
        }

        IsBusy = true;

        try
        {
            foreach (var field in ReportFields.Where(f => f.IsDirty && !string.IsNullOrWhiteSpace(f.FieldName)))
            {
                var command = new UpdateReportFieldCommand
                {
                    CommandId = Guid.NewGuid(),
                    ReportId = _reportId,
                    ExpectedVersion = _reportVersion,
                    FieldName = field.FieldName.Trim(),
                    Value = field.Value?.Trim() ?? string.Empty,
                    Confidence = 1.0
                };

                var result = await _reportService.UpdateFieldAsync(command);

                if (!result.IsSuccess)
                {
                    ErrorMessage = result.ErrorMessage ?? $"Не удалось сохранить поле {field.Keyword}.";
                    return;
                }

                _reportVersion++;
                field.OriginalValue = field.Value ?? string.Empty;
            }

            SuccessMessage = "Отчёт сохранён.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CompleteReportAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (_reportId == Guid.Empty)
        {
            ErrorMessage = "Отчёт не загружен.";
            return;
        }

        await SaveReportAsync();

        if (!string.IsNullOrWhiteSpace(ErrorMessage))
            return;

        IsBusy = true;

        try
        {
            var command = new CompleteReportCommand
            {
                CommandId = Guid.NewGuid(),
                ReportId = _reportId,
                ExpectedVersion = _reportVersion
            };

            var result = await _reportService.CompleteAsync(command);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.ErrorMessage;
                return;
            }

            _reportVersion++;
            IsCompleted = true;
            SuccessMessage = "Отчёт завершён.";
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

        if (!IsCompleted)
        {
            ErrorMessage = "PDF можно сформировать только для завершённого отчёта.";
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _reportService.DownloadPdfAsync(_reportId);

            if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.Data))
            {
                ErrorMessage = result.ErrorMessage ?? "Не удалось сформировать PDF.";
                return;
            }

            ReportApiService.OpenFile(result.Data);
            SuccessMessage = "PDF сформирован и открыт.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}