namespace UltrasoundAssistant.DoctorClient.Models.Statistics;

/// <summary>
/// Статистика по статусу отчёта.
/// </summary>
public sealed class ReportStatusStatisticsDto
{
    /// <summary>
    /// Техническое значение статуса отчёта.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Отображаемое название статуса отчёта.
    /// </summary>
    public string StatusDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Количество отчётов.
    /// </summary>
    public int Count { get; set; }
}
