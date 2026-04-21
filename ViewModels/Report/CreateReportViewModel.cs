using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;
using UltrasoundAssistant.DoctorClient.Models.Commands.Report;
using UltrasoundAssistant.DoctorClient.Models.Read.Patient;
using UltrasoundAssistant.DoctorClient.Models.Read.Template;
using UltrasoundAssistant.DoctorClient.Models.Voice;
using UltrasoundAssistant.DoctorClient.Services;
using UltrasoundAssistant.DoctorClient.Services.AudioService;

namespace UltrasoundAssistant.DoctorClient.ViewModels.Report;

public partial class CreateReportViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _main;
    private readonly PatientApiService _patientService;
    private readonly TemplateApiService _templateService;
    private readonly ReportApiService _reportService;
    private readonly VoiceApiService _voiceApiService;
    private readonly IAudioRecorderService _audioRecorderService;

    private Guid _currentReportId = Guid.Empty;
    private int _currentReportVersion = 0;

    public ObservableCollection<PatientDto> Patients { get; } = new();
    public ObservableCollection<TemplateDto> Templates { get; } = new();
    public ObservableCollection<ReportFieldEditorItem> ReportFields { get; } = new();

    [ObservableProperty]
    private PatientDto? selectedPatient;

    [ObservableProperty]
    private TemplateDto? selectedTemplate;

    [ObservableProperty]
    private string patientSearchText = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private string? successMessage;

    [ObservableProperty]
    private bool isReportCreated;

    [ObservableProperty]
    private bool isRecording;

    [ObservableProperty]
    private bool isPaused;

    [ObservableProperty]
    private string recognizedText = string.Empty;

    [ObservableProperty]
    private string unmatchedRecognitions = string.Empty;

    [ObservableProperty]
    private bool isRecognizing;

    public bool CanChangeReportSource => !IsReportCreated && !IsBusy && !IsRecognizing;
    public bool CanEditReportFields => IsReportCreated && !IsBusy && !IsRecognizing;
    public bool CanStartRecording => IsReportCreated && !IsBusy && !IsRecognizing && (!IsRecording || IsPaused);
    public bool CanPauseRecording => IsReportCreated && !IsBusy && !IsRecognizing && IsRecording && !IsPaused;
    public bool CanStopRecording => IsReportCreated && !IsBusy && !IsRecognizing && IsRecording;
    public bool CanSave => IsReportCreated && !IsBusy && !IsRecognizing;
    public bool CanComplete => IsReportCreated && !IsBusy && !IsRecognizing;

    public ICommand LoadInitialDataCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand SearchPatientsCommand { get; }
    public ICommand CreateReportCommand { get; }
    public ICommand StartRecordingCommand { get; }
    public ICommand PauseRecordingCommand { get; }
    public ICommand StopRecordingCommand { get; }
    public ICommand SaveReportCommand { get; }
    public ICommand CompleteReportCommand { get; }
    public ICommand GoBackCommand { get; }

    public CreateReportViewModel(
        MainWindowViewModel main,
        PatientApiService patientApiService,
        TemplateApiService templateApiService,
        ReportApiService reportApiService,
        VoiceApiService voiceApiService,
        IAudioRecorderService audioRecorderService)
    {
        _main = main;
        _patientService = patientApiService;
        _templateService = templateApiService;
        _reportService = reportApiService;
        _voiceApiService = voiceApiService;
        _audioRecorderService = audioRecorderService;

        LoadInitialDataCommand = new AsyncRelayCommand(LoadInitialDataAsync);
        RefreshCommand = new AsyncRelayCommand(LoadInitialDataAsync);
        SearchPatientsCommand = new AsyncRelayCommand(SearchPatientsAsync);
        CreateReportCommand = new AsyncRelayCommand(CreateReportAsync);
        StartRecordingCommand = new AsyncRelayCommand(StartRecordingAsync);
        PauseRecordingCommand = new AsyncRelayCommand(PauseRecordingAsync);
        StopRecordingCommand = new AsyncRelayCommand(StopRecordingAsync);
        SaveReportCommand = new AsyncRelayCommand(SaveReportAsync);
        CompleteReportCommand = new AsyncRelayCommand(CompleteReportAsync);
        GoBackCommand = main.GoBackCommand;
    }

    partial void OnIsReportCreatedChanged(bool value) => RaiseStateProperties();
    partial void OnIsRecordingChanged(bool value) => RaiseStateProperties();
    partial void OnIsPausedChanged(bool value) => RaiseStateProperties();
    partial void OnIsBusyChanged(bool value) => RaiseStateProperties();
    partial void OnIsRecognizingChanged(bool value) => RaiseStateProperties();

    private void RaiseStateProperties()
    {
        OnPropertyChanged(nameof(CanChangeReportSource));
        OnPropertyChanged(nameof(CanEditReportFields));
        OnPropertyChanged(nameof(CanStartRecording));
        OnPropertyChanged(nameof(CanPauseRecording));
        OnPropertyChanged(nameof(CanStopRecording));
    }

    private async Task LoadInitialDataAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;
        IsBusy = true;

        try
        {
            Patients.Clear();
            Templates.Clear();

            var patientsTask = _patientService.GetAllAsync();
            var templatesTask = _templateService.GetAllAsync();

            await Task.WhenAll(patientsTask, templatesTask);

            var patientsResult = await patientsTask;
            var templatesResult = await templatesTask;

            if (patientsResult.IsSuccess && patientsResult.Data != null)
            {
                foreach (var patient in patientsResult.Data)
                    Patients.Add(patient);
            }
            else
            {
                ErrorMessage = patientsResult.ErrorMessage ?? "Не удалось загрузить пациентов.";
            }

            if (templatesResult.IsSuccess && templatesResult.Data != null)
            {
                foreach (var template in templatesResult.Data)
                    Templates.Add(template);
            }
            else
            {
                ErrorMessage = string.IsNullOrWhiteSpace(ErrorMessage)
                    ? templatesResult.ErrorMessage ?? "Не удалось загрузить шаблоны."
                    : $"{ErrorMessage}{Environment.NewLine}{templatesResult.ErrorMessage}";
            }

            if (SelectedPatient == null && Patients.Count > 0)
                SelectedPatient = Patients[0];

            if (SelectedTemplate == null && Templates.Count > 0)
                SelectedTemplate = Templates[0];
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SearchPatientsAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;
        IsBusy = true;

        try
        {
            Patients.Clear();

            if (string.IsNullOrWhiteSpace(PatientSearchText))
            {
                var allResult = await _patientService.GetAllAsync();

                if (allResult.IsSuccess && allResult.Data != null)
                {
                    foreach (var patient in allResult.Data)
                        Patients.Add(patient);
                }
                else
                {
                    ErrorMessage = allResult.ErrorMessage;
                }

                return;
            }

            var result = await _patientService.SearchByFullNameAsync(PatientSearchText);

            if (result.IsSuccess && result.Data != null)
            {
                foreach (var patient in result.Data)
                    Patients.Add(patient);

                if (Patients.Count > 0)
                    SelectedPatient = Patients[0];
            }
            else
            {
                ErrorMessage = result.ErrorMessage;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CreateReportAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (SelectedPatient == null)
        {
            ErrorMessage = "Выберите пациента.";
            return;
        }

        if (SelectedTemplate == null)
        {
            ErrorMessage = "Выберите шаблон.";
            return;
        }

        var doctorId = ResolveCurrentDoctorId();
        if (doctorId == Guid.Empty)
        {
            ErrorMessage = "Не удалось определить текущего врача.";
            return;
        }

        IsBusy = true;

        try
        {
            var reportId = Guid.NewGuid();

            var command = new CreateReportCommand
            {
                CommandId = Guid.NewGuid(),
                ReportId = reportId,
                PatientId = SelectedPatient.Id,
                DoctorId = doctorId,
                TemplateId = SelectedTemplate.Id
            };

            var result = await _reportService.CreateAsync(command);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.ErrorMessage;
                return;
            }

            _currentReportId = reportId;
            _currentReportVersion = 1;

            BuildFieldsFromTemplate();
            RecognizedText = string.Empty;
            UnmatchedRecognitions = string.Empty;
            IsReportCreated = true;

            SuccessMessage = "Отчёт создан. Можно заполнять поля и запускать запись.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void BuildFieldsFromTemplate()
    {
        ReportFields.Clear();

        if (SelectedTemplate == null)
            return;

        var grouped = SelectedTemplate.Keywords
            .Where(k => !string.IsNullOrWhiteSpace(k.TargetField))
            .GroupBy(k => k.TargetField.Trim(), StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase);

        foreach (var group in grouped)
        {
            var firstKeyword = group.FirstOrDefault()?.Phrase?.Trim() ?? group.Key;

            ReportFields.Add(new ReportFieldEditorItem
            {
                FieldName = group.Key,
                Keyword = firstKeyword,
                Value = string.Empty,
                OriginalValue = string.Empty
            });
        }
    }

    private async Task StartRecordingAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (!IsReportCreated)
        {
            ErrorMessage = "Сначала создайте отчёт.";
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

        if (_currentReportId == Guid.Empty || SelectedTemplate == null)
        {
            ErrorMessage = "Отчёт ещё не создан.";
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

            var voiceRequest = new VoiceProcessRequest
            {
                ReportId = _currentReportId,
                TemplateId = SelectedTemplate.Id,
                AudioBase64 = Convert.ToBase64String(wavBytes),
                Language = "ru",
                FileName = $"report_{_currentReportId}.wav"
            };

            IsRecognizing = true;

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

        if (!IsReportCreated || _currentReportId == Guid.Empty)
        {
            ErrorMessage = "Сначала создайте отчёт.";
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
                    ReportId = _currentReportId,
                    ExpectedVersion = _currentReportVersion,
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

                _currentReportVersion++;
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

        if (!IsReportCreated || _currentReportId == Guid.Empty)
        {
            ErrorMessage = "Сначала создайте отчёт.";
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
                ReportId = _currentReportId,
                ExpectedVersion = _currentReportVersion
            };

            var result = await _reportService.CompleteAsync(command);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.ErrorMessage;
                return;
            }

            _currentReportVersion++;
            SuccessMessage = "Отчёт завершён.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private Guid ResolveCurrentDoctorId()
    {
        if (_main.CurrentUser is not null)
            return _main.CurrentUser.UserId;

        return Guid.Empty;
    }

    public void ResetForNewReport()
    {
        ErrorMessage = null;
        SuccessMessage = null;

        _currentReportId = Guid.Empty;
        _currentReportVersion = 0;

        IsReportCreated = false;
        IsRecording = false;
        IsPaused = false;
        IsRecognizing = false;
        IsBusy = false;

        RecognizedText = string.Empty;
        UnmatchedRecognitions = string.Empty;

        PatientSearchText = string.Empty;

        ReportFields.Clear();
    }
}