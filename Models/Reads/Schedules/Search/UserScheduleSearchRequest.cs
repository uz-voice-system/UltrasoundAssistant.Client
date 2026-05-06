using UltrasoundAssistant.DoctorClient.Models.Enums;

namespace UltrasoundAssistant.DoctorClient.Models.Reads.Schedules.Search;

/// <summary>
/// Фильтр поиска расписания пользователей.
/// </summary>
public sealed class UserScheduleSearchRequest
{
    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Роль пользователя.
    /// На текущем этапе чаще всего используется роль врача.
    /// </summary>
    public UserRole? UserRole { get; set; }

    /// <summary>
    /// День недели.
    /// </summary>
    public DayOfWeek? DayOfWeek { get; set; }

    /// <summary>
    /// Включать удалённые элементы расписания.
    /// </summary>
    public bool IncludeDeleted { get; set; }
}