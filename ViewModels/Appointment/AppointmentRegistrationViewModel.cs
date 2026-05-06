using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;
using UltrasoundAssistant.DoctorClient.Helpers;
using UltrasoundAssistant.DoctorClient.Models.Commands.Appointments;
using UltrasoundAssistant.DoctorClient.Models.Commands.Reports;
using UltrasoundAssistant.DoctorClient.Models.Enums;
using UltrasoundAssistant.DoctorClient.Models.Reads.Appointments.Search;
using UltrasoundAssistant.DoctorClient.Models.Reads.Patients.Search;
using UltrasoundAssistant.DoctorClient.Models.Reads.Templates.Search;
using UltrasoundAssistant.DoctorClient.Models.Reads.Users.Search;
using UltrasoundAssistant.DoctorClient.Services;

namespace UltrasoundAssistant.DoctorClient.ViewModels.Appointment;

public class AppointmentRegistrationViewModel : CrudPageViewModelBase<AppointmentSummaryDto>
{
    private readonly PatientApiService _patientService;
    private readonly UserApiService _userService;
    private readonly TemplateApiService _templateService;
    private readonly AppointmentApiService _appointmentService;
    private readonly ReportApiService _reportService;

    public ObservableCollection<AppointmentSummaryDto> Appointments => Items;

    public ObservableCollection<PatientSummaryDto> Patients { get; } = new();
    public ObservableCollection<UserSummaryDto> Doctors { get; } = new();
    public ObservableCollection<TemplateSummaryDto> Templates { get; } = new();
    public ObservableCollection<AppointmentStatus?> Statuses { get; } = new();

    private bool _areAdditionalFiltersVisible;
    public bool AreAdditionalFiltersVisible
    {
        get => _areAdditionalFiltersVisible;
        set => SetProperty(ref _areAdditionalFiltersVisible, value);
    }

    private string _patientSearchText = string.Empty;
    public string PatientSearchText
    {
        get => _patientSearchText;
        set => SetProperty(ref _patientSearchText, value);
    }

    private string _doctorSearchText = string.Empty;
    public string DoctorSearchText
    {
        get => _doctorSearchText;
        set => SetProperty(ref _doctorSearchText, value);
    }

    private string _templateSearchText = string.Empty;
    public string TemplateSearchText
    {
        get => _templateSearchText;
        set => SetProperty(ref _templateSearchText, value);
    }

    private PatientSummaryDto? _selectedPatient;
    public PatientSummaryDto? SelectedPatient
    {
        get => _selectedPatient;
        set => SetProperty(ref _selectedPatient, value);
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
        set
        {
            if (SetProperty(ref _selectedTemplate, value) && value != null)
            {
                DurationMinutes = value.DefaultAppointmentDurationMinutes > 0
                    ? value.DefaultAppointmentDurationMinutes
                    : 30;
            }
        }
    }

    private DateTimeOffset? _appointmentDate = DateTimeOffset.Now.Date;
    public DateTimeOffset? AppointmentDate
    {
        get => _appointmentDate;
        set => SetProperty(ref _appointmentDate, value);
    }

    private TimeSpan? _appointmentTime = new(9, 0, 0);
    public TimeSpan? AppointmentTime
    {
        get => _appointmentTime;
        set => SetProperty(ref _appointmentTime, value);
    }

    private int _durationMinutes = 30;
    public int DurationMinutes
    {
        get => _durationMinutes;
        set => SetProperty(ref _durationMinutes, value);
    }

    private string _comment = string.Empty;
    public string Comment
    {
        get => _comment;
        set => SetProperty(ref _comment, value);
    }

    private string _appointmentSearchText = string.Empty;
    public string AppointmentSearchText
    {
        get => _appointmentSearchText;
        set => SetProperty(ref _appointmentSearchText, value);
    }

    private DateTimeOffset? _filterFromDate = DateTimeOffset.Now.Date;
    public DateTimeOffset? FilterFromDate
    {
        get => _filterFromDate;
        set => SetProperty(ref _filterFromDate, value);
    }

    private DateTimeOffset? _filterToDate = DateTimeOffset.Now.Date.AddDays(1);
    public DateTimeOffset? FilterToDate
    {
        get => _filterToDate;
        set => SetProperty(ref _filterToDate, value);
    }

    private AppointmentStatus? _filterStatus;
    public AppointmentStatus? FilterStatus
    {
        get => _filterStatus;
        set => SetProperty(ref _filterStatus, value);
    }

    private bool _includeDeleted;
    public bool IncludeDeleted
    {
        get => _includeDeleted;
        set => SetProperty(ref _includeDeleted, value);
    }

    public ICommand LoadInitialDataCommand { get; }
    public ICommand SearchPatientsCommand { get; }
    public ICommand SearchDoctorsCommand { get; }
    public ICommand SearchTemplatesCommand { get; }
    public ICommand SearchAppointmentsCommand { get; }
    public ICommand CreateAppointmentCommand { get; }
    public ICommand ClearAppointmentFiltersCommand { get; }
    public ICommand ToggleAdditionalFiltersCommand { get; }

    public AppointmentRegistrationViewModel(
        MainWindowViewModel main,
        PatientApiService patientService,
        UserApiService userService,
        TemplateApiService templateService,
        AppointmentApiService appointmentService,
        ReportApiService reportService)
        : base(main)
    {
        _patientService = patientService;
        _userService = userService;
        _templateService = templateService;
        _appointmentService = appointmentService;
        _reportService = reportService;

        Statuses.Add(null);
        foreach (var status in Enum.GetValues<AppointmentStatus>())
            Statuses.Add(status);

        LoadInitialDataCommand = new AsyncRelayCommand(LoadInitialDataAsync);
        SearchPatientsCommand = new AsyncRelayCommand(SearchPatientsAsync);
        SearchDoctorsCommand = new AsyncRelayCommand(SearchDoctorsAsync);
        SearchTemplatesCommand = new AsyncRelayCommand(SearchTemplatesAsync);
        SearchAppointmentsCommand = new AsyncRelayCommand(SearchAppointmentsAsync);
        CreateAppointmentCommand = new AsyncRelayCommand(CreateAppointmentAsync);
        ClearAppointmentFiltersCommand = new AsyncRelayCommand(ClearAppointmentFiltersAsync);

        ToggleAdditionalFiltersCommand = new RelayCommandSync(_ => { AreAdditionalFiltersVisible = !AreAdditionalFiltersVisible; });
    }

    private async Task LoadInitialDataAsync()
    {
        ClearError();

        await SearchPatientsAsync();
        await SearchDoctorsAsync();
        await SearchTemplatesAsync();
        await SearchAppointmentsAsync();
    }

    private async Task SearchPatientsAsync()
    {
        var result = await _patientService.SearchAsync(new PatientSearchRequest
        {
            SearchText = string.IsNullOrWhiteSpace(PatientSearchText) ? null : PatientSearchText.Trim(),
            IncludeDeleted = false
        });

        Patients.Clear();

        if (result.IsSuccess && result.Data != null)
        {
            foreach (var patient in result.Data)
                Patients.Add(patient);
        }
        else
        {
            SetError(result.ErrorMessage);
        }
    }

    private async Task SearchDoctorsAsync()
    {
        var result = await _userService.SearchAsync(new UserSearchRequest
        {
            SearchText = string.IsNullOrWhiteSpace(DoctorSearchText) ? null : DoctorSearchText.Trim(),
            Role = UserRole.Doctor,
            IsActive = true
        });

        Doctors.Clear();

        if (result.IsSuccess && result.Data != null)
        {
            foreach (var doctor in result.Data)
                Doctors.Add(doctor);
        }
        else
        {
            SetError(result.ErrorMessage);
        }
    }

    private async Task SearchTemplatesAsync()
    {
        var result = await _templateService.SearchForDoctorAsync(new TemplateSearchRequest
        {
            SearchText = string.IsNullOrWhiteSpace(TemplateSearchText) ? null : TemplateSearchText.Trim(),
            IncludeDeleted = false
        });

        Templates.Clear();

        if (result.IsSuccess && result.Data != null)
        {
            foreach (var template in result.Data)
                Templates.Add(template);
        }
        else
        {
            SetError(result.ErrorMessage);
        }
    }

    private async Task SearchAppointmentsAsync()
    {
        ClearError();

        var filter = new AppointmentSearchRequest
        {
            SearchText = string.IsNullOrWhiteSpace(AppointmentSearchText) ? null : AppointmentSearchText.Trim(),
            PatientId = SelectedPatient?.Id,
            DoctorId = AreAdditionalFiltersVisible ? SelectedDoctor?.Id : null,
            TemplateId = AreAdditionalFiltersVisible ? SelectedTemplate?.Id : null,
            Status = AreAdditionalFiltersVisible ? FilterStatus : null,
            FromUtc = FilterFromDate?.DateTime.Date.ToUniversalTime(),
            ToUtc = FilterToDate?.DateTime.Date.AddDays(1).ToUniversalTime(),
            IncludeDeleted = IncludeDeleted
        };

        var result = await _appointmentService.SearchAsync(filter);

        if (result.IsSuccess && result.Data != null)
            ReplaceItems(result.Data);
        else
            SetError(result.ErrorMessage);
    }

    private async Task ClearAppointmentFiltersAsync()
    {
        AppointmentSearchText = string.Empty;
        FilterFromDate = DateTimeOffset.Now.Date;
        FilterToDate = DateTimeOffset.Now.Date.AddDays(1);
        FilterStatus = null;
        IncludeDeleted = false;

        await SearchAppointmentsAsync();
    }

    private async Task CreateAppointmentAsync()
    {
        ClearError();

        if (SelectedPatient == null)
        {
            SetError("Выберите пациента.");
            return;
        }

        if (SelectedDoctor == null)
        {
            SetError("Выберите врача.");
            return;
        }

        if (SelectedTemplate == null)
        {
            SetError("Выберите шаблон исследования.");
            return;
        }

        if (AppointmentDate == null)
        {
            SetError("Укажите дату приёма.");
            return;
        }

        if (AppointmentTime == null)
        {
            SetError("Укажите время приёма.");
            return;
        }

        if (DurationMinutes <= 0)
        {
            SetError("Длительность приёма должна быть больше нуля.");
            return;
        }

        var localStart = AppointmentDate.Value.Date + AppointmentTime.Value;
        var localEnd = localStart.AddMinutes(DurationMinutes);

        var startAtUtc = localStart.ToUniversalTime();
        var endAtUtc = localEnd.ToUniversalTime();

        var appointmentId = Guid.NewGuid();

        var appointmentCommand = new CreateAppointmentCommand
        {
            AppointmentId = appointmentId,
            PatientId = SelectedPatient.Id,
            DoctorId = SelectedDoctor.Id,
            TemplateId = SelectedTemplate.Id,
            CreatedByUserId = Main.CurrentUser.UserId,
            StartAtUtc = startAtUtc,
            EndAtUtc = endAtUtc,
            Comment = string.IsNullOrWhiteSpace(Comment) ? null : Comment.Trim()
        };

        var appointmentResult = await _appointmentService.CreateAsync(appointmentCommand);

        if (!appointmentResult.IsSuccess)
        {
            SetError(appointmentResult.ErrorMessage);
            return;
        }

        var reportCommand = new CreateReportCommand
        {
            ReportId = Guid.NewGuid(),
            AppointmentId = appointmentId,
            Status = ReportStatus.Draft,
            ContentJson = "{}"
        };

        var reportResult = await _reportService.CreateAsync(reportCommand);

        if (!reportResult.IsSuccess)
        {
            SetError(reportResult.ErrorMessage ?? "Запись создана, но не удалось создать отчёт-черновик.");
            await SearchAppointmentsAsync();
            return;
        }

        Comment = string.Empty;
        await SearchAppointmentsAsync();
    }
}
