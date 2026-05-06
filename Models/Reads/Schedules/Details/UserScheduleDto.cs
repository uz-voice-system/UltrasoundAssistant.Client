namespace UltrasoundAssistant.DoctorClient.Models.Reads.Schedules.Details;

/// <summary>
/// Элемент расписания пользователя.
/// </summary>
public sealed class UserScheduleDto
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
    /// День недели.
    /// </summary>
    public DayOfWeek DayOfWeek { get; set; }

    /// <summary>
    /// Время начала работы.
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// Время окончания работы.
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
