using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;
using UltrasoundAssistant.DoctorClient.Helpers;
using UltrasoundAssistant.DoctorClient.Models.Commands.Schedules;
using UltrasoundAssistant.DoctorClient.Models.Commands.Users;
using UltrasoundAssistant.DoctorClient.Models.Entity.Schedules;
using UltrasoundAssistant.DoctorClient.Models.Entity.Users;
using UltrasoundAssistant.DoctorClient.Models.Enums;
using UltrasoundAssistant.DoctorClient.Models.Reads.Schedules.Search;
using UltrasoundAssistant.DoctorClient.Models.Reads.Users.Search;
using UltrasoundAssistant.DoctorClient.Services;

namespace UltrasoundAssistant.DoctorClient.ViewModels.User;

public class UsersAdminViewModel : CrudPageViewModelBase<UserSummaryDto>
{
    private readonly UserApiService _userService;
    private readonly ScheduleApiService _scheduleService;

    private Guid? _editingUserId;
    private int _editingUserVersion;
    private int _editingScheduleVersion;

    public ObservableCollection<UserSummaryDto> Users => Items;
    public ObservableCollection<UserRole?> Roles { get; } = new();
    public ObservableCollection<BoolFilterOption> ActiveOptions { get; } = new();
    public ObservableCollection<EditableUserScheduleItem> ScheduleItems { get; } = new();

    private bool _areAdditionalFiltersVisible;
    public bool AreAdditionalFiltersVisible
    {
        get => _areAdditionalFiltersVisible;
        set => SetProperty(ref _areAdditionalFiltersVisible, value);
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private UserRole? _filterRole;
    public UserRole? FilterRole
    {
        get => _filterRole;
        set => SetProperty(ref _filterRole, value);
    }

    private BoolFilterOption? _selectedActiveOption;
    public BoolFilterOption? SelectedActiveOption
    {
        get => _selectedActiveOption;
        set => SetProperty(ref _selectedActiveOption, value);
    }

    private string _filterLogin = string.Empty;
    public string FilterLogin
    {
        get => _filterLogin;
        set => SetProperty(ref _filterLogin, value);
    }

    private string _filterFullName = string.Empty;
    public string FilterFullName
    {
        get => _filterFullName;
        set => SetProperty(ref _filterFullName, value);
    }

    private string _editLogin = string.Empty;
    public string EditLogin
    {
        get => _editLogin;
        set => SetProperty(ref _editLogin, value);
    }

    private string _editPassword = string.Empty;
    public string EditPassword
    {
        get => _editPassword;
        set => SetProperty(ref _editPassword, value);
    }

    private string _editFullName = string.Empty;
    public string EditFullName
    {
        get => _editFullName;
        set => SetProperty(ref _editFullName, value);
    }

    private UserRole? _editRole;
    public UserRole? EditRole
    {
        get => _editRole;
        set
        {
            if (SetProperty(ref _editRole, value))
                OnPropertyChanged(nameof(IsDoctorRoleSelected));
        }
    }

    private bool _editIsActive = true;
    public bool EditIsActive
    {
        get => _editIsActive;
        set => SetProperty(ref _editIsActive, value);
    }

    private string _editSpecialization = string.Empty;
    public string EditSpecialization
    {
        get => _editSpecialization;
        set => SetProperty(ref _editSpecialization, value);
    }

    private string _editCabinet = string.Empty;
    public string EditCabinet
    {
        get => _editCabinet;
        set => SetProperty(ref _editCabinet, value);
    }

    private string _editPhoneExtension = string.Empty;
    public string EditPhoneExtension
    {
        get => _editPhoneExtension;
        set => SetProperty(ref _editPhoneExtension, value);
    }

    public bool IsDoctorRoleSelected => EditRole == UserRole.Doctor;

    public ICommand LoadUsersCommand { get; }
    public ICommand SearchUsersCommand { get; }
    public ICommand ClearFiltersCommand { get; }
    public ICommand ToggleAdditionalFiltersCommand { get; }

    public ICommand AddUserCommand { get; }
    public ICommand EditUserCommand { get; }
    public ICommand SaveUserCommand { get; }
    public ICommand CancelEditCommand { get; }

    public ICommand ActivateUserCommand { get; }
    public ICommand DeactivateUserCommand { get; }

    public UsersAdminViewModel(
        MainWindowViewModel main,
        UserApiService userService,
        ScheduleApiService scheduleService)
        : base(main)
    {
        _userService = userService;
        _scheduleService = scheduleService;

        Roles.Add(null);
        foreach (var role in Enum.GetValues<UserRole>())
            Roles.Add(role);

        ActiveOptions.Add(new BoolFilterOption("Все", null));
        ActiveOptions.Add(new BoolFilterOption("Активные", true));
        ActiveOptions.Add(new BoolFilterOption("Неактивные", false));
        SelectedActiveOption = ActiveOptions[0];

        ResetScheduleItems();

        LoadUsersCommand = new AsyncRelayCommand(LoadUsersAsync);
        SearchUsersCommand = new AsyncRelayCommand(SearchUsersAsync);
        ClearFiltersCommand = new AsyncRelayCommand(ClearFiltersAsync);

        ToggleAdditionalFiltersCommand = new RelayCommandSync(_ =>
        {
            AreAdditionalFiltersVisible = !AreAdditionalFiltersVisible;
        });

        AddUserCommand = new RelayCommandSync(_ => OpenEditPanelForAdd());

        EditUserCommand = new RelayCommand<UserSummaryDto?>(async user =>
        {
            await OpenEditPanelForEditAsync(user);
        });

        SaveUserCommand = new AsyncRelayCommand(SaveUserAsync);
        CancelEditCommand = new RelayCommandSync(_ => CloseEditPanel());

        ActivateUserCommand = new RelayCommand<UserSummaryDto?>(async user =>
        {
            await ActivateUserAsync(user);
        });

        DeactivateUserCommand = new RelayCommand<UserSummaryDto?>(async user =>
        {
            await DeactivateUserAsync(user);
        });
    }

    private async Task LoadUsersAsync()
    {
        ClearError();
        IsEditPanelVisible = false;

        var result = await _userService.GetAllAsync();

        if (result.IsSuccess && result.Data != null)
            ReplaceItems(result.Data);
        else
            SetError(result.ErrorMessage);
    }

    private async Task SearchUsersAsync()
    {
        ClearError();

        var filter = new UserSearchRequest
        {
            SearchText = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim(),
            Role = FilterRole,
            IsActive = SelectedActiveOption?.Value,
            Login = AreAdditionalFiltersVisible && !string.IsNullOrWhiteSpace(FilterLogin)
                ? FilterLogin.Trim()
                : null,
            FullName = AreAdditionalFiltersVisible && !string.IsNullOrWhiteSpace(FilterFullName)
                ? FilterFullName.Trim()
                : null
        };

        var result = await _userService.SearchAsync(filter);

        if (result.IsSuccess && result.Data != null)
            ReplaceItems(result.Data);
        else
            SetError(result.ErrorMessage);
    }

    private async Task ClearFiltersAsync()
    {
        SearchText = string.Empty;
        FilterRole = null;
        SelectedActiveOption = ActiveOptions[0];
        FilterLogin = string.Empty;
        FilterFullName = string.Empty;

        await LoadUsersAsync();
    }

    private void OpenEditPanelForAdd()
    {
        _editingUserId = null;
        _editingUserVersion = 0;
        _editingScheduleVersion = 0;

        OpenEditPanel("Добавить пользователя");

        EditLogin = string.Empty;
        EditPassword = string.Empty;
        EditFullName = string.Empty;
        EditRole = UserRole.Doctor;
        EditIsActive = true;

        EditSpecialization = string.Empty;
        EditCabinet = string.Empty;
        EditPhoneExtension = string.Empty;

        ResetScheduleItems();
    }

    private async Task OpenEditPanelForEditAsync(UserSummaryDto? user)
    {
        ClearError();

        if (user == null)
            return;

        var result = await _userService.GetByIdAsync(user.Id);

        if (!result.IsSuccess || result.Data == null)
        {
            SetError(result.ErrorMessage ?? "Не удалось загрузить пользователя.");
            return;
        }

        var full = result.Data;

        _editingUserId = full.Id;
        _editingUserVersion = full.Version;

        OpenEditPanel("Редактировать пользователя");

        EditLogin = full.Login;
        EditPassword = string.Empty;
        EditFullName = full.FullName;
        EditRole = full.Role;
        EditIsActive = full.IsActive;

        EditSpecialization = full.DoctorProfile?.Specialization ?? string.Empty;
        EditCabinet = full.DoctorProfile?.Cabinet ?? string.Empty;
        EditPhoneExtension = full.DoctorProfile?.PhoneExtension ?? string.Empty;

        ResetScheduleItems();

        if (full.Role == UserRole.Doctor)
            await LoadScheduleAsync(full.Id);
    }

    protected override void CloseEditPanel()
    {
        base.CloseEditPanel();

        _editingUserId = null;
        _editingUserVersion = 0;
        _editingScheduleVersion = 0;

        EditLogin = string.Empty;
        EditPassword = string.Empty;
        EditFullName = string.Empty;
        EditRole = null;
        EditIsActive = true;

        EditSpecialization = string.Empty;
        EditCabinet = string.Empty;
        EditPhoneExtension = string.Empty;

        ResetScheduleItems();
    }

    private async Task SaveUserAsync()
    {
        ClearError();

        if (string.IsNullOrWhiteSpace(EditLogin))
        {
            SetError("Логин обязателен.");
            return;
        }

        if (string.IsNullOrWhiteSpace(EditFullName))
        {
            SetError("ФИО обязательно.");
            return;
        }

        if (EditRole == null)
        {
            SetError("Выберите роль пользователя.");
            return;
        }

        if (_editingUserId == null && string.IsNullOrWhiteSpace(EditPassword))
        {
            SetError("Для нового пользователя пароль обязателен.");
            return;
        }

        var userId = _editingUserId ?? Guid.NewGuid();

        var doctorProfile = EditRole == UserRole.Doctor
            ? new DoctorProfileDto
            {
                UserId = userId,
                Specialization = string.IsNullOrWhiteSpace(EditSpecialization) ? null : EditSpecialization.Trim(),
                Cabinet = string.IsNullOrWhiteSpace(EditCabinet) ? null : EditCabinet.Trim(),
                PhoneExtension = string.IsNullOrWhiteSpace(EditPhoneExtension) ? null : EditPhoneExtension.Trim()
            }
            : null;

        if (_editingUserId == null)
        {
            var command = new CreateUserCommand
            {
                UserId = userId,
                Login = EditLogin.Trim(),
                Password = EditPassword,
                FullName = EditFullName.Trim(),
                Role = EditRole.Value,
                DoctorProfile = doctorProfile
            };

            var result = await _userService.CreateAsync(command);

            if (!result.IsSuccess)
            {
                SetError(result.ErrorMessage);
                return;
            }

            if (EditRole == UserRole.Doctor)
            {
                var scheduleSaved = await SaveScheduleAsync(userId, 0);

                if (!scheduleSaved)
                    return;
            }
        }
        else
        {
            var command = new UpdateUserCommand
            {
                UserId = _editingUserId.Value,
                Login = EditLogin.Trim(),
                Password = string.IsNullOrWhiteSpace(EditPassword) ? null : EditPassword,
                FullName = EditFullName.Trim(),
                Role = EditRole.Value,
                IsActive = EditIsActive,
                DoctorProfile = doctorProfile,
                ExpectedVersion = _editingUserVersion
            };

            var result = await _userService.UpdateAsync(command);

            if (!result.IsSuccess)
            {
                SetError(result.ErrorMessage);
                return;
            }

            if (EditRole == UserRole.Doctor)
            {
                var scheduleSaved = await SaveScheduleAsync(_editingUserId.Value, _editingScheduleVersion);

                if (!scheduleSaved)
                    return;
            }
        }

        CloseEditPanel();
        await SearchUsersAsync();
    }

    private async Task<bool> SaveScheduleAsync(Guid userId, int expectedVersion)
    {
        var enabledItems = ScheduleItems
            .Where(x => x.IsEnabled)
            .ToList();

        foreach (var item in enabledItems)
        {
            if (item.StartTime == null || item.EndTime == null)
            {
                SetError($"Для дня {item.DayName} нужно указать время начала и окончания.");
                return false;
            }

            if (item.StartTime >= item.EndTime)
            {
                SetError($"Для дня {item.DayName} время начала должно быть меньше времени окончания.");
                return false;
            }
        }

        var command = new UpdateUserScheduleCommand
        {
            UserId = userId,
            ExpectedVersion = expectedVersion,
            Items = BuildScheduleItems(enabledItems)
        };

        var result = await _scheduleService.UpdateAsync(command);

        if (!result.IsSuccess)
        {
            SetError(result.ErrorMessage ?? "Не удалось сохранить расписание врача.");
            return false;
        }

        return true;
    }

    private static List<UserScheduleItemDto> BuildScheduleItems(List<EditableUserScheduleItem> enabledItems)
    {
        return enabledItems
            .Select(x => new UserScheduleItemDto
            {
                ScheduleId = x.ScheduleId == Guid.Empty ? Guid.NewGuid() : x.ScheduleId,
                DayOfWeek = x.DayOfWeek,
                StartTime = x.StartTime!.Value,
                EndTime = x.EndTime!.Value
            })
            .ToList();
    }

    private async Task LoadScheduleAsync(Guid userId)
    {
        _editingScheduleVersion = 0;

        var result = await _scheduleService.SearchAsync(new UserScheduleSearchRequest
        {
            UserId = userId,
            UserRole = UserRole.Doctor,
            IncludeDeleted = false
        });

        if (!result.IsSuccess || result.Data == null)
            return;

        _editingScheduleVersion = result.Data.Count == 0
            ? 0
            : result.Data.Max(x => x.Version);

        foreach (var schedule in result.Data.Where(x => !x.IsDeleted))
        {
            var item = ScheduleItems.FirstOrDefault(x => x.DayOfWeek == schedule.DayOfWeek);

            if (item == null)
                continue;

            item.ScheduleId = schedule.Id;
            item.IsEnabled = true;
            item.StartTime = schedule.StartTime;
            item.EndTime = schedule.EndTime;
        }
    }

    private async Task ActivateUserAsync(UserSummaryDto? user)
    {
        ClearError();

        if (user == null)
            return;

        var result = await _userService.ActivateAsync(new ActivateUserCommand
        {
            UserId = user.Id,
            ExpectedVersion = user.Version
        });

        if (result.IsSuccess)
            await SearchUsersAsync();
        else
            SetError(result.ErrorMessage);
    }

    private async Task DeactivateUserAsync(UserSummaryDto? user)
    {
        ClearError();

        if (user == null)
            return;

        var result = await _userService.DeactivateAsync(new DeactivateUserCommand
        {
            UserId = user.Id,
            ExpectedVersion = user.Version
        });

        if (result.IsSuccess)
            await SearchUsersAsync();
        else
            SetError(result.ErrorMessage);
    }

    private void ResetScheduleItems()
    {
        ScheduleItems.Clear();

        AddScheduleDay(DayOfWeek.Monday, "Понедельник");
        AddScheduleDay(DayOfWeek.Tuesday, "Вторник");
        AddScheduleDay(DayOfWeek.Wednesday, "Среда");
        AddScheduleDay(DayOfWeek.Thursday, "Четверг");
        AddScheduleDay(DayOfWeek.Friday, "Пятница");
        AddScheduleDay(DayOfWeek.Saturday, "Суббота");
        AddScheduleDay(DayOfWeek.Sunday, "Воскресенье");
    }

    private void AddScheduleDay(DayOfWeek dayOfWeek, string dayName)
    {
        ScheduleItems.Add(new EditableUserScheduleItem
        {
            ScheduleId = Guid.Empty,
            DayOfWeek = dayOfWeek,
            DayName = dayName,
            IsEnabled = dayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday),
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(18, 0, 0)
        });
    }
}
