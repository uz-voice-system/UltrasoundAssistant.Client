namespace UltrasoundAssistant.DoctorClient.Models.Statistics;

/// <summary>
/// Статистика по шаблону исследования.
/// </summary>
public sealed class TemplateStatisticsDto
{
    /// <summary>
    /// Идентификатор шаблона.
    /// </summary>
    public Guid TemplateId { get; set; }

    /// <summary>
    /// Название шаблона.
    /// </summary>
    public string TemplateName { get; set; } = string.Empty;

    /// <summary>
    /// Количество записей на приём.
    /// </summary>
    public int AppointmentsCount { get; set; }

    /// <summary>
    /// Количество принятых записей.
    /// </summary>
    public int AcceptedAppointmentsCount { get; set; }

    /// <summary>
    /// Количество записей в процессе приёма.
    /// </summary>
    public int InProgressAppointmentsCount { get; set; }

    /// <summary>
    /// Количество завершённых записей.
    /// </summary>
    public int CompletedAppointmentsCount { get; set; }

    /// <summary>
    /// Количество неявок.
    /// </summary>
    public int NoShowAppointmentsCount { get; set; }

    /// <summary>
    /// Количество уникальных принятых пациентов.
    /// </summary>
    public int UniqueAcceptedPatientsCount { get; set; }

    /// <summary>
    /// Количество созданных отчётов.
    /// </summary>
    public int ReportsCount { get; set; }
}
