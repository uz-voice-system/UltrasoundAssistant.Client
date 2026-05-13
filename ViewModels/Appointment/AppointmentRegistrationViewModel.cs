using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;
using UltrasoundAssistant.DoctorClient.Helpers;
using UltrasoundAssistant.DoctorClient.Models.Commands.Appointments;
using UltrasoundAssistant.DoctorClient.Models.Commands.Reports;
using UltrasoundAssistant.DoctorClient.Models.Enums;
using UltrasoundAssistant.DoctorClient.Models.Reads.Appointments.Details;
using UltrasoundAssistant.DoctorClient.Models.Reads.Appointments.Search;
using UltrasoundAssistant.DoctorClient.Models.Reads.Patients.Search;
using UltrasoundAssistant.DoctorClient.Models.Reads.Schedules.Search;
using UltrasoundAssistant.DoctorClient.Models.Reads.Templates.Search;
using UltrasoundAssistant.DoctorClient.Models.Reads.Users.Search;
using UltrasoundAssistant.DoctorClient.Services;

namespace UltrasoundAssistant.DoctorClient.ViewModels.Appointment;

public class AppointmentRegistrationViewModel : CrudPageViewModelBase<AppointmentCardViewItem>
{
    private readonly MainWindowViewModel _main;
    private readonly PatientApiService _patientService;
    private readonly UserApiService _userService;
    private readonly TemplateApiService _templateService;
    private readonly AppointmentApiService _appointmentService;
    private readonly ReportApiService _reportService;
    private readonly ScheduleApiService _scheduleService;

    public ObservableCollection<AppointmentCardViewItem> Appointments => Items;

    public ObservableCollection<PatientSummaryDto> Patients { get; } = new();
    public ObservableCollection<UserSummaryDto> Doctors { get; } = new();
    public ObservableCollection<TemplateSummaryDto> Templates { get; } = new();

    public ObservableCollection<string> StatusFilterOptions { get; } = new();

    public ObservableCollection<DoctorDayScheduleViewItem> DoctorScheduleDays { get; } = new();

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    private string? _successMessage;
    public string? SuccessMessage
    {
        get => _successMessage;
        set => SetProperty(ref _successMessage, value);
    }

    private bool _areAdditionalFiltersVisible;
    public bool AreAdditionalFiltersVisible
    {
        get => _areAdditionalFiltersVisible;
        set => SetProperty(ref _areAdditionalFiltersVisible, value);
    }

    private bool _isDoctorScheduleVisible;
    public bool IsDoctorScheduleVisible
    {
        get => _isDoctorScheduleVisible;
        set
        {
            if (SetProperty(ref _isDoctorScheduleVisible, value))
                OnPropertyChanged(nameof(IsAppointmentsPanelVisible));
        }
    }

    public bool IsAppointmentsPanelVisible => !IsDoctorScheduleVisible;

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
        set
        {
            if (SetProperty(ref _selectedDoctor, value))
            {
                IsDoctorScheduleVisible = value != null;

                if (value == null)
                    DoctorScheduleDays.Clear();
                else
                    _ = RefreshDoctorScheduleAsync();
            }
        }
    }

    private TemplateSummaryDto? _selectedTemplate;
    public TemplateSummaryDto? SelectedTemplate
    {
        get => _selectedTemplate;
        set
        {
            if (SetProperty(ref _selectedTemplate, value))
                ResetDurationFromTemplate();
        }
    }

    private DateTimeOffset? _appointmentDate = DateTimeOffset.Now.Date;
    public DateTimeOffset? AppointmentDate
    {
        get => _appointmentDate;
        set
        {
            if (SetProperty(ref _appointmentDate, value))
                _ = RefreshDoctorScheduleAsync();
        }
    }

    private TimeSpan? _appointmentTime = new(9, 0, 0);
    public TimeSpan? AppointmentTime
    {
        get => _appointmentTime;
        set
        {
            if (SetProperty(ref _appointmentTime, value))
                _ = RefreshDoctorScheduleAsync();
        }
    }

    private int _durationMinutes = 30;
    public int DurationMinutes
    {
        get => _durationMinutes;
        set
        {
            if (SetProperty(ref _durationMinutes, value))
                _ = RefreshDoctorScheduleAsync();
        }
    }

    private string _comment = string.Empty;
    public string Comment
    {
        get => _comment;
        set => SetProperty(ref _comment, value);
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

    private string _filterStatusText = "Все статусы";
    public string FilterStatusText
    {
        get => _filterStatusText;
        set => SetProperty(ref _filterStatusText, value);
    }

    private UserSummaryDto? _filterDoctor;
    public UserSummaryDto? FilterDoctor
    {
        get => _filterDoctor;
        set => SetProperty(ref _filterDoctor, value);
    }

    private TemplateSummaryDto? _filterTemplate;
    public TemplateSummaryDto? FilterTemplate
    {
        get => _filterTemplate;
        set => SetProperty(ref _filterTemplate, value);
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
    public ICommand ClearAppointmentFiltersCommand { get; }

    public ICommand CreateAppointmentCommand { get; }
    public ICommand CancelAppointmentCommand { get; }

    public ICommand ResetNewAppointmentCommand { get; }
    public ICommand ResetDoctorScheduleViewCommand { get; }
    public ICommand ResetDurationFromTemplateCommand { get; }
    public ICommand SelectTimeSlotCommand { get; }

    public AppointmentRegistrationViewModel(
        MainWindowViewModel main,
        PatientApiService patientService,
        UserApiService userService,
        TemplateApiService templateService,
        AppointmentApiService appointmentService,
        ReportApiService reportService,
        ScheduleApiService scheduleService)
        : base(main)
    {
        _main = main;
        _patientService = patientService;
        _userService = userService;
        _templateService = templateService;
        _appointmentService = appointmentService;
        _reportService = reportService;
        _scheduleService = scheduleService;

        StatusFilterOptions.Add("Все статусы");
        StatusFilterOptions.Add("Запланирована");
        StatusFilterOptions.Add("В процессе");
        StatusFilterOptions.Add("Завершена");
        StatusFilterOptions.Add("Отменена");
        StatusFilterOptions.Add("Неявка");
        FilterStatusText = StatusFilterOptions[0];

        LoadInitialDataCommand = new AsyncRelayCommand(LoadInitialDataAsync);
        SearchPatientsCommand = new AsyncRelayCommand(SearchPatientsAsync);
        SearchDoctorsCommand = new AsyncRelayCommand(SearchDoctorsAsync);
        SearchTemplatesCommand = new AsyncRelayCommand(SearchTemplatesAsync);
        SearchAppointmentsCommand = new AsyncRelayCommand(SearchAppointmentsAsync);
        ClearAppointmentFiltersCommand = new AsyncRelayCommand(ClearAppointmentFiltersAsync);

        CreateAppointmentCommand = new AsyncRelayCommand(CreateAppointmentAsync);
        CancelAppointmentCommand = new RelayCommand<AppointmentCardViewItem?>(async item => await CancelAppointmentAsync(item));

        ResetNewAppointmentCommand = new AsyncRelayCommand(ResetNewAppointmentAsync);
        ResetDoctorScheduleViewCommand = new RelayCommandSync(_ => ResetDoctorScheduleView());
        ResetDurationFromTemplateCommand = new RelayCommandSync(_ => ResetDurationFromTemplate());
        SelectTimeSlotCommand = new RelayCommand<DoctorTimeSlotViewItem?>(SelectTimeSlot);

        _ = LoadInitialDataSafeAsync();
    }

    private async Task LoadInitialDataSafeAsync()
    {
        try
        {
            await LoadInitialDataAsync();
        }
        catch (Exception ex)
        {
            SetError($"Ошибка загрузки данных: {ex.Message}");
        }
    }

    public async Task LoadInitialDataAsync()
    {
        ClearError();
        SuccessMessage = null;

        await SearchPatientsAsync();
        await SearchDoctorsAsync();
        await SearchTemplatesAsync();
        await SearchAppointmentsAsync();
    }

    private async Task SearchPatientsAsync()
    {
        ClearError();

        var result = await _patientService.SearchAsync(new PatientSearchRequest
        {
            SearchText = string.IsNullOrWhiteSpace(PatientSearchText) ? null : PatientSearchText.Trim(),
            IncludeDeleted = false
        });

        Patients.Clear();

        if (result.IsSuccess && result.Data != null)
        {
            foreach (var patient in result.Data.OrderBy(x => x.FullName))
                Patients.Add(patient);
        }
        else
        {
            SetError(result.ErrorMessage);
        }
    }

    private async Task SearchDoctorsAsync()
    {
        ClearError();

        var result = await _userService.SearchAsync(new UserSearchRequest
        {
            SearchText = string.IsNullOrWhiteSpace(DoctorSearchText) ? null : DoctorSearchText.Trim(),
            Role = UserRole.Doctor,
            IsActive = true
        });

        Doctors.Clear();

        if (result.IsSuccess && result.Data != null)
        {
            foreach (var doctor in result.Data.OrderBy(x => x.FullName))
                Doctors.Add(doctor);
        }
        else
        {
            SetError(result.ErrorMessage);
        }
    }

    private async Task SearchTemplatesAsync()
    {
        ClearError();

        var result = await _templateService.SearchForDoctorAsync(new TemplateSearchRequest
        {
            SearchText = string.IsNullOrWhiteSpace(TemplateSearchText) ? null : TemplateSearchText.Trim(),
            IncludeDeleted = false
        });

        Templates.Clear();

        if (result.IsSuccess && result.Data != null)
        {
            foreach (var template in result.Data.OrderBy(x => x.Name))
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

        DateTime? fromUtc = FilterFromDate.HasValue
            ? ToUtcStartOfLocalDate(FilterFromDate.Value.Date)
            : null;

        DateTime? toUtc = FilterToDate.HasValue
            ? ToUtcStartOfLocalDate(FilterToDate.Value.Date.AddDays(1))
            : null;

        if (fromUtc != null && toUtc != null && fromUtc > toUtc)
        {
            SetError("Дата начала периода не может быть позже даты окончания.");
            return;
        }

        var filter = new AppointmentSearchRequest
        {
            DoctorId = AreAdditionalFiltersVisible ? FilterDoctor?.Id : null,
            TemplateId = AreAdditionalFiltersVisible ? FilterTemplate?.Id : null,
            Status = AreAdditionalFiltersVisible ? MapNullableStatus(FilterStatusText) : null,
            FromUtc = fromUtc,
            ToUtc = toUtc,
            IncludeDeleted = IncludeDeleted
        };

        var result = await _appointmentService.SearchAsync(filter);

        if (result.IsSuccess && result.Data != null)
        {
            ReplaceAppointmentItems(result.Data);
        }
        else
        {
            ReplaceItems([]);
            SetError(result.ErrorMessage);
        }
    }

    private void ReplaceAppointmentItems(List<AppointmentSummaryDto> appointments)
    {
        ReplaceItems(appointments
            .OrderBy(x => x.StartAtUtc)
            .Select(x => new AppointmentCardViewItem(x))
            .ToList());
    }

    private async Task ClearAppointmentFiltersAsync()
    {
        FilterFromDate = DateTimeOffset.Now.Date;
        FilterToDate = DateTimeOffset.Now.Date;
        FilterStatusText = StatusFilterOptions[0];
        FilterDoctor = null;
        FilterTemplate = null;
        IncludeDeleted = false;

        await SearchAppointmentsAsync();
    }

    private async Task ResetNewAppointmentAsync()
    {
        ClearError();
        SuccessMessage = null;

        SelectedPatient = null;
        SelectedDoctor = null;
        SelectedTemplate = null;

        PatientSearchText = string.Empty;
        DoctorSearchText = string.Empty;
        TemplateSearchText = string.Empty;

        AppointmentDate = DateTimeOffset.Now.Date;
        AppointmentTime = new TimeSpan(9, 0, 0);
        DurationMinutes = 30;
        Comment = string.Empty;

        ResetDoctorScheduleView();

        await SearchPatientsAsync();
        await SearchDoctorsAsync();
        await SearchTemplatesAsync();
    }

    private void ResetDoctorScheduleView()
    {
        SelectedDoctor = null;
        DoctorScheduleDays.Clear();
        IsDoctorScheduleVisible = false;
    }

    private void ResetDurationFromTemplate()
    {
        DurationMinutes = SelectedTemplate?.DefaultAppointmentDurationMinutes > 0
            ? SelectedTemplate.DefaultAppointmentDurationMinutes
            : 30;
    }

    private void SelectTimeSlot(DoctorTimeSlotViewItem? slot)
    {
        if (slot == null || !slot.CanSelect)
            return;

        AppointmentDate = new DateTimeOffset(slot.StartLocal.Date);
        AppointmentTime = slot.StartLocal.TimeOfDay;
    }

    private async Task CreateAppointmentAsync()
    {
        ClearError();
        SuccessMessage = null;

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

        if (localStart < DateTime.Now)
        {
            SetError("Нельзя записать пациента на прошедшее время.");
            return;
        }

        if (!await ValidateDoctorAvailabilityAsync(SelectedDoctor.Id, localStart, localEnd))
            return;

        var appointmentId = Guid.NewGuid();

        var appointmentCommand = new CreateAppointmentCommand
        {
            AppointmentId = appointmentId,
            PatientId = SelectedPatient.Id,
            DoctorId = SelectedDoctor.Id,
            TemplateId = SelectedTemplate.Id,
            CreatedByUserId = _main.CurrentUser.UserId,
            StartAtUtc = ToUtcLocalDateTime(localStart),
            EndAtUtc = ToUtcLocalDateTime(localEnd),
            Comment = string.IsNullOrWhiteSpace(Comment) ? null : Comment.Trim()
        };

        IsBusy = true;

        try
        {
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
                RefreshLaterIfCurrent(SearchAppointmentsAsync);
                return;
            }

            Comment = string.Empty;
            SuccessMessage = "Пациент записан, отчёт-черновик создан.";

            RefreshLaterIfCurrent(async () =>
            {
                await SearchAppointmentsAsync();
                await RefreshDoctorScheduleAsync();
            });
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<bool> ValidateDoctorAvailabilityAsync(Guid doctorId, DateTime localStart, DateTime localEnd)
    {
        if (localStart < DateTime.Now)
        {
            SetError("Нельзя записать пациента на прошедшее время.");
            return false;
        }

        if (localEnd <= localStart)
        {
            SetError("Время окончания должно быть позже времени начала.");
            return false;
        }

        if (localStart.Date != localEnd.Date)
        {
            SetError("Запись должна находиться в пределах одного дня.");
            return false;
        }

        var scheduleResult = await _scheduleService.SearchAsync(new UserScheduleSearchRequest
        {
            UserId = doctorId,
            DayOfWeek = localStart.DayOfWeek,
            IncludeDeleted = false
        });

        var schedule = scheduleResult.IsSuccess && scheduleResult.Data != null
            ? scheduleResult.Data.FirstOrDefault(x => !x.IsDeleted && x.DayOfWeek == localStart.DayOfWeek)
            : null;

        if (schedule == null)
        {
            SetError("В выбранный день врач не работает.");
            return false;
        }

        if (localStart.TimeOfDay < schedule.StartTime || localEnd.TimeOfDay > schedule.EndTime)
        {
            SetError($"Выбранное время вне рабочего расписания врача: {schedule.StartTime:hh\\:mm}–{schedule.EndTime:hh\\:mm}.");
            return false;
        }

        var appointmentsResult = await _appointmentService.SearchAsync(new AppointmentSearchRequest
        {
            DoctorId = doctorId,
            FromUtc = ToUtcStartOfLocalDate(localStart.Date),
            ToUtc = ToUtcStartOfLocalDate(localStart.Date.AddDays(1)),
            IncludeDeleted = false
        });

        if (!appointmentsResult.IsSuccess || appointmentsResult.Data == null)
            return true;

        var conflicted = appointmentsResult.Data.FirstOrDefault(x =>
        {
            if (x.Status is AppointmentStatus.Canceled or AppointmentStatus.NoShow)
                return false;

            var existingStart = ToLocalFromUtc(x.StartAtUtc);
            var existingEnd = ToLocalFromUtc(x.EndAtUtc);

            return existingStart < localEnd && existingEnd > localStart;
        });

        if (conflicted != null)
        {
            var existingStart = ToLocalFromUtc(conflicted.StartAtUtc);
            var existingEnd = ToLocalFromUtc(conflicted.EndAtUtc);

            SetError(
                $"Выбранное время пересекается с другой записью: " +
                $"{existingStart:HH:mm}–{existingEnd:HH:mm}, пациент: {conflicted.PatientFullName}.");

            return false;
        }

        return true;
    }

    private async Task CancelAppointmentAsync(AppointmentCardViewItem? item)
    {
        ClearError();
        SuccessMessage = null;

        if (item == null)
            return;

        if (!item.CanCancel)
        {
            SetError("Отменить можно только запланированную запись.");
            return;
        }

        IsBusy = true;

        try
        {
            var fullResult = await _appointmentService.GetByIdAsync(item.Id);

            if (!fullResult.IsSuccess || fullResult.Data == null)
            {
                SetError(fullResult.ErrorMessage ?? "Не удалось загрузить запись.");
                return;
            }

            var appointment = fullResult.Data;

            var command = new UpdateAppointmentCommand
            {
                AppointmentId = appointment.Id,
                PatientId = appointment.PatientId,
                DoctorId = appointment.DoctorId,
                TemplateId = appointment.TemplateId,
                StartAtUtc = appointment.StartAtUtc,
                EndAtUtc = appointment.EndAtUtc,
                Status = AppointmentStatus.Canceled,
                Comment = appointment.Comment,
                ExpectedVersion = appointment.Version
            };

            var result = await _appointmentService.UpdateAsync(command);

            if (!result.IsSuccess)
            {
                SetError(result.ErrorMessage);
                return;
            }

            SuccessMessage = "Запись отменена.";

            RefreshLaterIfCurrent(async () =>
            {
                await SearchAppointmentsAsync();
                await RefreshDoctorScheduleAsync();
            });
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RefreshDoctorScheduleAsync()
    {
        if (SelectedDoctor == null || AppointmentDate == null)
            return;

        var selectedLocalDate = AppointmentDate.Value.Date;
        var weekStart = GetWeekStartMonday(selectedLocalDate);
        var weekEndExclusive = weekStart.AddDays(7);

        var scheduleResult = await _scheduleService.SearchAsync(new UserScheduleSearchRequest
        {
            UserId = SelectedDoctor.Id,
            IncludeDeleted = false
        });

        var scheduleItems = scheduleResult.IsSuccess && scheduleResult.Data != null
            ? scheduleResult.Data.Where(x => !x.IsDeleted).ToList()
            : [];

        var appointmentsResult = await _appointmentService.SearchAsync(new AppointmentSearchRequest
        {
            DoctorId = SelectedDoctor.Id,
            FromUtc = ToUtcStartOfLocalDate(weekStart),
            ToUtc = ToUtcStartOfLocalDate(weekEndExclusive),
            IncludeDeleted = false
        });

        var appointments = appointmentsResult.IsSuccess && appointmentsResult.Data != null
            ? appointmentsResult.Data
            : [];

        DoctorScheduleDays.Clear();

        for (var i = 0; i < 7; i++)
        {
            var day = weekStart.AddDays(i);
            var daySchedule = scheduleItems.FirstOrDefault(x => x.DayOfWeek == day.DayOfWeek);

            var dayItem = new DoctorDayScheduleViewItem
            {
                Date = day,
                DayName = GetDayName(day.DayOfWeek),
                DateText = day.ToString("dd.MM.yyyy"),
                IsSelectedDay = day.Date == selectedLocalDate.Date
            };

            if (daySchedule == null)
            {
                dayItem.StatusText = "Врач не работает";
                dayItem.StatusBackground = Brush.Parse("#F0F0F0");
                dayItem.StatusForeground = Brush.Parse("#777777");

                DoctorScheduleDays.Add(dayItem);
                continue;
            }

            dayItem.StatusText = $"{daySchedule.StartTime:hh\\:mm}–{daySchedule.EndTime:hh\\:mm}";
            dayItem.StatusBackground = Brush.Parse("#E8F1FF");
            dayItem.StatusForeground = Brush.Parse("#2457A6");

            BuildDaySlots(dayItem, daySchedule.StartTime, daySchedule.EndTime, appointments);

            DoctorScheduleDays.Add(dayItem);
        }
    }

    private void BuildDaySlots(
        DoctorDayScheduleViewItem dayItem,
        TimeSpan workStart,
        TimeSpan workEnd,
        List<AppointmentSummaryDto> appointments)
    {
        var slotMinutes = DurationMinutes > 0 ? DurationMinutes : 30;

        var dayStart = dayItem.Date.Date + workStart;
        var dayEnd = dayItem.Date.Date + workEnd;

        var selectedStart = AppointmentDate != null && AppointmentTime != null
            ? AppointmentDate.Value.Date + AppointmentTime.Value
            : (DateTime?)null;

        for (var slotStart = dayStart; slotStart.AddMinutes(slotMinutes) <= dayEnd; slotStart = slotStart.AddMinutes(slotMinutes))
        {
            var slotEnd = slotStart.AddMinutes(slotMinutes);

            var busyAppointment = appointments.FirstOrDefault(x =>
            {
                if (x.Status is AppointmentStatus.Canceled or AppointmentStatus.NoShow)
                    return false;

                var appointmentStart = ToLocalFromUtc(x.StartAtUtc);
                var appointmentEnd = ToLocalFromUtc(x.EndAtUtc);

                return appointmentStart < slotEnd && appointmentEnd > slotStart;
            });

            var isBusy = busyAppointment != null;
            var isPast = slotStart < DateTime.Now;
            var isSelected = selectedStart != null && selectedStart.Value == slotStart;

            dayItem.Slots.Add(new DoctorTimeSlotViewItem
            {
                StartLocal = slotStart,
                EndLocal = slotEnd,
                TimeText = $"{slotStart:HH:mm}–{slotEnd:HH:mm}",
                IsBusy = isBusy,
                IsPast = isPast,
                IsSelected = isSelected,
                PatientText = busyAppointment == null ? string.Empty : busyAppointment.PatientFullName
            });
        }
    }

    private static DateTime GetWeekStartMonday(DateTime date)
    {
        var localDate = date.Date;
        var diff = ((int)localDate.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;

        return localDate.AddDays(-diff);
    }

    private static DateTime ToUtcStartOfLocalDate(DateTime localDate)
    {
        return ToUtcLocalDateTime(localDate.Date);
    }

    private static DateTime ToUtcLocalDateTime(DateTime localDateTime)
    {
        var unspecified = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(unspecified, TimeZoneInfo.Local);
    }

    private static DateTime ToLocalFromUtc(DateTime utcDateTime)
    {
        var utc = utcDateTime.Kind == DateTimeKind.Utc
            ? utcDateTime
            : DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

        return utc.ToLocalTime();
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

    private static string GetStatusText(AppointmentStatus status)
    {
        return status switch
        {
            AppointmentStatus.Scheduled => "Запланирована",
            AppointmentStatus.InProgress => "В процессе",
            AppointmentStatus.Completed => "Завершена",
            AppointmentStatus.Canceled => "Отменена",
            AppointmentStatus.NoShow => "Неявка",
            _ => status.ToString()
        };
    }

    private static string GetDayName(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => "Понедельник",
            DayOfWeek.Tuesday => "Вторник",
            DayOfWeek.Wednesday => "Среда",
            DayOfWeek.Thursday => "Четверг",
            DayOfWeek.Friday => "Пятница",
            DayOfWeek.Saturday => "Суббота",
            DayOfWeek.Sunday => "Воскресенье",
            _ => dayOfWeek.ToString()
        };
    }

    public static string LocalizeStatus(AppointmentStatus status)
    {
        return GetStatusText(status);
    }
}
