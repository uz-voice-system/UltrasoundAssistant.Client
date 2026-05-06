namespace UltrasoundAssistant.DoctorClient.Models.Reads.Templates.Search;

/// <summary>
/// Краткая информация о шаблоне
/// </summary>
public sealed class TemplateSummaryDto
{
    /// <summary>
    /// Идентификатор шаблона
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Название шаблона
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Длительность приёма по умолчанию в минутах.
    /// </summary>
    public int DefaultAppointmentDurationMinutes { get; set; }

    /// <summary>
    /// Признак удаления
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Версия шаблона
    /// </summary>
    public int Version { get; set; }
}
