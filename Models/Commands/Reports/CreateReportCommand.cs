using UltrasoundAssistant.DoctorClient.Models.Enums;

namespace UltrasoundAssistant.DoctorClient.Models.Commands.Reports;

/// <summary>
/// Команда создания отчёта
/// </summary>
public sealed class CreateReportCommand
{
    /// <summary>
    /// Идентификатор отчёта
    /// </summary>
    public Guid ReportId { get; set; }

    /// <summary>
    /// Идентификатор записи на приём
    /// </summary>
    public Guid AppointmentId { get; set; }

    /// <summary>
    /// Статус отчёта
    /// </summary>
    public ReportStatus Status { get; set; }

    /// <summary>
    /// Содержимое отчёта в формате JSON
    /// </summary>
    public string ContentJson { get; set; } = "{}";
}
