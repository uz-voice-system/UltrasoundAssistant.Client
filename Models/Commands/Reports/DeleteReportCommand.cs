namespace UltrasoundAssistant.DoctorClient.Models.Commands.Reports;

/// <summary>
/// Команда удаления отчёта
/// </summary>
public sealed class DeleteReportCommand
{
    /// <summary>
    /// Идентификатор отчёта
    /// </summary>
    public Guid ReportId { get; set; }

    /// <summary>
    /// Ожидаемая версия агрегата
    /// </summary>
    public int ExpectedVersion { get; set; }
}
