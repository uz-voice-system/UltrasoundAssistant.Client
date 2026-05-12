namespace UltrasoundAssistant.DoctorClient.Models.Statistics;

/// <summary>
/// Статистика администратора.
/// </summary>
public sealed class AdminStatisticsDto
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
    /// Общее количество записей на приём.
    /// </summary>
    public int TotalAppointmentsCount { get; set; }

    /// <summary>
    /// Количество принятых записей.
    /// Принятой считается запись в статусе InProgress или Completed.
    /// </summary>
    public int AcceptedAppointmentsCount { get; set; }

    /// <summary>
    /// Количество уникальных принятых пациентов.
    /// </summary>
    public int UniqueAcceptedPatientsCount { get; set; }

    /// <summary>
    /// Количество созданных отчётов.
    /// </summary>
    public int ReportsCount { get; set; }

    /// <summary>
    /// Количество принятых записей без отчёта.
    /// Например, запись уже InProgress, но отчёт ещё не создан.
    /// </summary>
    public int AppointmentsWithoutReportCount { get; set; }

    /// <summary>
    /// Количество запланированных записей.
    /// </summary>
    public int ScheduledAppointmentsCount { get; set; }

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
    /// Статистика по врачам.
    /// </summary>
    public List<DoctorStatisticsDto> Doctors { get; set; } = [];

    /// <summary>
    /// Статистика по шаблонам исследований.
    /// </summary>
    public List<TemplateStatisticsDto> Templates { get; set; } = [];

    /// <summary>
    /// Статистика по статусам записей.
    /// </summary>
    public List<AppointmentStatusStatisticsDto> AppointmentStatuses { get; set; } = [];

    /// <summary>
    /// Статистика по статусам отчётов.
    /// </summary>
    public List<ReportStatusStatisticsDto> ReportStatuses { get; set; } = [];
}
