using UltrasoundAssistant.DoctorClient.Models.Entity.Schedules;

namespace UltrasoundAssistant.DoctorClient.Models.Commands.Schedules;

/// <summary>
/// Команда обновления расписания пользователя
/// </summary>
public sealed class UpdateUserScheduleCommand
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Элементы расписания
    /// </summary>
    public List<UserScheduleItemDto> Items { get; set; } = [];

    /// <summary>
    /// Ожидаемая версия агрегата
    /// </summary>
    public int ExpectedVersion { get; set; }
}
