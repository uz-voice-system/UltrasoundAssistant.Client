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
using SharedBoolFilterOption = UltrasoundAssistant.DoctorClient.ViewModels.BoolFilterOption;

namespace UltrasoundAssistant.DoctorClient.ViewModels.User;

public class UsersAdminViewModel : CrudPageViewModelBase<UserAdminListItem>
{
    private readonly MainWindowViewModel _main;
    private readonly UserApiService _userService;
    private readonly ScheduleApiService _scheduleService;

    private Guid? _editingUserId;
    private int _editingUserVersion;
    private int _editingScheduleVersion;

    public ObservableCollection<UserAdminListItem> Users => Items;

    public ObservableCollection<string> RoleFilterOptions { get; } = new();
    public ObservableCollection<string> RoleEditOptions { get; } = new();

    public ObservableCollection<SharedBoolFilterOption> ActiveOptions { get; } = new();
    public ObservableCollection<EditableUserScheduleItem> ScheduleItems { get; } = new();

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private string _filterRoleText = "Все роли";
    public string FilterRoleText
    {
        get => _filterRoleText;
        set => SetProperty(ref _filterRoleText, value);
    }

    private SharedBoolFilterOption? _selectedActiveOption;
    public SharedBoolFilterOption? SelectedActiveOption
    {
        get => _selectedActiveOption;
        set => SetProperty(ref _selectedActiveOption, value);
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

    private string _editRoleText = "Врач";
    public string EditRoleText
    {
        get => _editRoleText;
        set
        {
            if (SetProperty(ref _editRoleText, value))
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

    public bool IsDoctorRoleSelected => MapRole(EditRoleText) == UserRole.Doctor;

    public ICommand LoadUsersCommand { get; }
    public ICommand SearchUsersCommand { get; }
    public ICommand ClearFiltersCommand { get; }

    public ICommand AddUserCommand { get; }
    public ICommand EditUserCommand { get; }
    public ICommand SaveUserCommand { get; }
    public ICommand CancelEditCommand { get; }

    public ICommand ToggleUserActivityCommand { get; }

    public UsersAdminViewModel(
        MainWindowViewModel main,
        UserApiService userService,
        ScheduleApiService scheduleService)
        : base(main)
    {
        _main = main;
        _userService = userService;
        _scheduleService = scheduleService;

        RoleFilterOptions.Add("Все роли");
        RoleFilterOptions.Add("Врач");
        RoleFilterOptions.Add("Администратор");
        RoleFilterOptions.Add("Регистратор");
        FilterRoleText = RoleFilterOptions[0];

        RoleEditOptions.Add("Врач");
        RoleEditOptions.Add("Администратор");
        RoleEditOptions.Add("Регистратор");
        EditRoleText = RoleEditOptions[0];

        ActiveOptions.Add(new SharedBoolFilterOption("Все статусы", null));
        ActiveOptions.Add(new SharedBoolFilterOption("Активные", true));
        ActiveOptions.Add(new SharedBoolFilterOption("Неактивные", false));
        SelectedActiveOption = ActiveOptions[0];

        ResetScheduleItems();

        LoadUsersCommand = new AsyncRelayCommand(LoadUsersAsync);
        SearchUsersCommand = new AsyncRelayCommand(SearchUsersAsync);
        ClearFiltersCommand = new AsyncRelayCommand(ClearFiltersAsync);

        AddUserCommand = new RelayCommandSync(_ => OpenEditPanelForAdd());

        EditUserCommand = new RelayCommand<UserAdminListItem?>(async user =>
        {
            await OpenEditPanelForEditAsync(user);
        });

        SaveUserCommand = new AsyncRelayCommand(SaveUserAsync);
        CancelEditCommand = new RelayCommandSync(_ => CloseEditPanel());

        ToggleUserActivityCommand = new RelayCommand<UserAdminListItem?>(async user =>
        {
            await ToggleUserActivityAsync(user);
        });
    }

    private async Task LoadUsersAsync()
    {
        ClearError();
        IsEditPanelVisible = false;

        var result = await _userService.GetAllAsync();

        if (result.IsSuccess && result.Data != null)
            ReplaceUsers(result.Data);
        else
            SetError(result.ErrorMessage);
    }

    private async Task SearchUsersAsync()
    {
        ClearError();

        var filter = new UserSearchRequest
        {
            SearchText = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim(),
            Role = MapNullableRole(FilterRoleText),
            IsActive = SelectedActiveOption?.Value
        };

        var result = await _userService.SearchAsync(filter);

        if (result.IsSuccess && result.Data != null)
            ReplaceUsers(result.Data);
        else
            SetError(result.ErrorMessage);
    }

    private void ReplaceUsers(List<UserSummaryDto> users)
    {
        var currentUserId = _main.CurrentUser.UserId;

        ReplaceItems(users
            .Select(x => new UserAdminListItem(x, currentUserId))
            .OrderByDescending(x => x.IsCurrentUser)
            .ThenBy(x => x.FullName)
            .ToList());
    }

    private async Task ClearFiltersAsync()
    {
        SearchText = string.Empty;
        FilterRoleText = RoleFilterOptions[0];
        SelectedActiveOption = ActiveOptions[0];

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
        EditRoleText = "Врач";
        EditIsActive = true;

        EditSpecialization = string.Empty;
        EditCabinet = string.Empty;
        EditPhoneExtension = string.Empty;

        ResetScheduleItems();
    }

    private async Task OpenEditPanelForEditAsync(UserAdminListItem? user)
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
        EditRoleText = GetRoleText(full.Role);
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
        EditRoleText = "Врач";
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

        if (_editingUserId == null && string.IsNullOrWhiteSpace(EditPassword))
        {
            SetError("Для нового пользователя пароль обязателен.");
            return;
        }

        var role = MapRole(EditRoleText);
        var userId = _editingUserId ?? Guid.NewGuid();

        var doctorProfile = role == UserRole.Doctor
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
                Role = role,
                DoctorProfile = doctorProfile
            };

            var result = await _userService.CreateAsync(command);

            if (!result.IsSuccess)
            {
                SetError(result.ErrorMessage);
                return;
            }

            if (role == UserRole.Doctor)
            {
                var scheduleSaved = await SaveScheduleAsync(userId, 1);

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
                Role = role,
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

            if (role == UserRole.Doctor)
            {
                var expectedVersion = _editingScheduleVersion <= 0 ? 1 : _editingScheduleVersion;
                var scheduleSaved = await SaveScheduleAsync(_editingUserId.Value, expectedVersion);

                if (!scheduleSaved)
                    return;
            }
        }

        CloseEditPanel();
        RefreshLaterIfCurrent(SearchUsersAsync);
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

            var start = NormalizeScheduleTime(item.StartTime.Value);
            var end = NormalizeScheduleTime(item.EndTime.Value);

            if (start >= end)
            {
                SetError($"Для дня {item.DayName} время начала должно быть меньше времени окончания.");
                return false;
            }

            item.StartTime = start;
            item.EndTime = end;
        }

        var versionsToTry = expectedVersion <= 0
            ? new[] { 1, 0 }
            : expectedVersion == 1
                ? new[] { 1, 0 }
                : new[] { expectedVersion };

        string? lastError = null;

        foreach (var version in versionsToTry)
        {
            var command = new UpdateUserScheduleCommand
            {
                UserId = userId,
                ExpectedVersion = version,
                Items = BuildScheduleItems(enabledItems)
            };

            var result = await _scheduleService.UpdateAsync(command);

            if (result.IsSuccess)
                return true;

            lastError = result.ErrorMessage;

            if (string.IsNullOrWhiteSpace(result.ErrorMessage) ||
                !result.ErrorMessage.Contains("Concurrency conflict", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }
        }

        SetError(lastError ?? "Не удалось сохранить расписание врача.");
        return false;
    }

    private static List<UserScheduleItemDto> BuildScheduleItems(List<EditableUserScheduleItem> enabledItems)
    {
        return enabledItems
            .Select(x => new UserScheduleItemDto
            {
                ScheduleId = x.ScheduleId == Guid.Empty ? Guid.NewGuid() : x.ScheduleId,
                DayOfWeek = x.DayOfWeek,
                StartTime = NormalizeScheduleTime(x.StartTime!.Value),
                EndTime = NormalizeScheduleTime(x.EndTime!.Value)
            })
            .ToList();
    }

    private async Task LoadScheduleAsync(Guid userId)
    {
        _editingScheduleVersion = 0;

        var result = await _scheduleService.SearchAsync(new UserScheduleSearchRequest
        {
            UserId = userId,
            IncludeDeleted = false
        });

        if (!result.IsSuccess || result.Data == null)
        {
            SetError(result.ErrorMessage ?? "Не удалось загрузить расписание врача.");
            return;
        }

        _editingScheduleVersion = result.Data.Count == 0
            ? 1
            : result.Data.Max(x => x.Version);

        foreach (var schedule in result.Data.Where(x => !x.IsDeleted))
        {
            var item = ScheduleItems.FirstOrDefault(x => x.DayOfWeek == schedule.DayOfWeek);

            if (item == null)
                continue;

            item.ScheduleId = schedule.Id;
            item.IsEnabled = true;
            item.StartTime = NormalizeScheduleTime(schedule.StartTime);
            item.EndTime = NormalizeScheduleTime(schedule.EndTime);
        }
    }

    private async Task ToggleUserActivityAsync(UserAdminListItem? user)
    {
        ClearError();

        if (user == null)
            return;

        if (user.IsActive)
        {
            var deactivateResult = await _userService.DeactivateAsync(new DeactivateUserCommand
            {
                UserId = user.Id,
                ExpectedVersion = user.Version
            });

            if (!deactivateResult.IsSuccess)
            {
                SetError(deactivateResult.ErrorMessage);
                return;
            }
        }
        else
        {
            var activateResult = await _userService.ActivateAsync(new ActivateUserCommand
            {
                UserId = user.Id,
                ExpectedVersion = user.Version
            });

            if (!activateResult.IsSuccess)
            {
                SetError(activateResult.ErrorMessage);
                return;
            }
        }

        RefreshLaterIfCurrent(SearchUsersAsync);
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
            IsEnabled = false,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(18, 0, 0)
        });
    }

    private static UserRole? MapNullableRole(string? roleText)
    {
        return roleText switch
        {
            "Врач" => UserRole.Doctor,
            "Администратор" => UserRole.Admin,
            "Регистратор" => UserRole.Registrar,
            _ => null
        };
    }

    private static UserRole MapRole(string roleText)
    {
        return roleText switch
        {
            "Врач" => UserRole.Doctor,
            "Администратор" => UserRole.Admin,
            "Регистратор" => UserRole.Registrar,
            _ => UserRole.Doctor
        };
    }

    private static string GetRoleText(UserRole role)
    {
        return role switch
        {
            UserRole.Doctor => "Врач",
            UserRole.Admin => "Администратор",
            UserRole.Registrar => "Регистратор",
            _ => role.ToString()
        };
    }

    private static TimeSpan NormalizeScheduleTime(TimeSpan value)
    {
        return new TimeSpan(value.Hours, value.Minutes, 0);
    }
}
