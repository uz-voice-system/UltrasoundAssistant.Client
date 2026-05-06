namespace UltrasoundAssistant.DoctorClient.Models.Reads.Schedules.Search;

/// <summary>
/// Краткая информация об элементе расписания.
/// </summary>
public sealed class UserScheduleSummaryDto
{
    /// <summary>
    /// Идентификатор элемента расписания.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// ФИО пользователя.
    /// </summary>
    public string UserFullName { get; set; } = string.Empty;

    /// <summary>
    /// День недели.
    /// </summary>
    public DayOfWeek DayOfWeek { get; set; }

    /// <summary>
    /// Время начала.
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// Время окончания.
    /// </summary>
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// Признак удаления.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Версия агрегата.
    /// </summary>
    public int Version { get; set; }
}
