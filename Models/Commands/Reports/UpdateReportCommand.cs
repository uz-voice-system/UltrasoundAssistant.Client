using UltrasoundAssistant.DoctorClient.Models.Enums;

namespace UltrasoundAssistant.DoctorClient.Models.Commands.Reports;

/// <summary>
/// Команда обновления отчёта
/// </summary>
public sealed class UpdateReportCommand
{
    /// <summary>
    /// Идентификатор отчёта
    /// </summary>
    public Guid ReportId { get; set; }

    /// <summary>
    /// Статус отчёта
    /// </summary>
    public ReportStatus Status { get; set; }

    /// <summary>
    /// Содержимое отчёта в формате JSON
    /// </summary>
    public string ContentJson { get; set; } = "{}";

    /// <summary>
    /// Ожидаемая версия агрегата
    /// </summary>
    public int ExpectedVersion { get; set; }
}
