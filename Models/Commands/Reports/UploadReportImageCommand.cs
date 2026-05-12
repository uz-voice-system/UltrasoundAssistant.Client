namespace UltrasoundAssistant.DoctorClient.Models.Commands.Reports;

/// <summary>
/// Команда загрузки изображения УЗИ для отчёта.
/// </summary>
public sealed class UploadReportImageCommand
{
    /// <summary>
    /// Идентификатор отчёта.
    /// </summary>
    public Guid ReportId { get; set; }

    /// <summary>
    /// Название файла.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// MIME-тип изображения.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Изображение в формате Base64.
    /// </summary>
    public string ImageBase64 { get; set; } = string.Empty;

    /// <summary>
    /// Ожидаемая версия отчёта.
    /// </summary>
    public int ExpectedVersion { get; set; }
}
