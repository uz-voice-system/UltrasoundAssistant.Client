namespace UltrasoundAssistant.DoctorClient.Models.Commands.Reports;

/// <summary>
/// Команда удаления изображения УЗИ из отчёта.
/// </summary>
public sealed class DeleteReportImageCommand
{
    /// <summary>
    /// Идентификатор отчёта.
    /// </summary>
    public Guid ReportId { get; set; }

    /// <summary>
    /// Ожидаемая версия отчёта.
    /// </summary>
    public int ExpectedVersion { get; set; }
}
