namespace UltrasoundAssistant.DoctorClient.Models.Statistics;

/// <summary>
/// Статистика по статусу записи на приём.
/// </summary>
public sealed class AppointmentStatusStatisticsDto
{
    /// <summary>
    /// Техническое значение статуса.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Отображаемое название статуса.
    /// </summary>
    public string StatusDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Количество записей.
    /// </summary>
    public int Count { get; set; }
}
