namespace UltrasoundAssistant.DoctorClient.Models.Statistics;

/// <summary>
/// Запрос статистики администратора.
/// </summary>
public sealed class AdminStatisticsRequest
{
    /// <summary>
    /// Дата начала периода.
    /// </summary>
    public DateTime DateFromUtc { get; set; }

    /// <summary>
    /// Дата окончания периода.
    /// </summary>
    public DateTime DateToUtc { get; set; }

    /// <summary>
    /// Идентификатор врача.
    /// Если задан, статистика будет построена только по нему.
    /// </summary>
    public Guid? DoctorId { get; set; }

    /// <summary>
    /// Идентификатор шаблона исследования.
    /// Если задан, статистика будет построена только по нему.
    /// </summary>
    public Guid? TemplateId { get; set; }
}
