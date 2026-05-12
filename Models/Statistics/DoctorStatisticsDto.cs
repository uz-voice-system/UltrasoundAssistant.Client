namespace UltrasoundAssistant.DoctorClient.Models.Statistics;

/// <summary>
/// Статистика по врачу.
/// </summary>
public sealed class DoctorStatisticsDto
{
    /// <summary>
    /// Идентификатор врача.
    /// </summary>
    public Guid DoctorId { get; set; }

    /// <summary>
    /// ФИО врача.
    /// </summary>
    public string DoctorFullName { get; set; } = string.Empty;

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
