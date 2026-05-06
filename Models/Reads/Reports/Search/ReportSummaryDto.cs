using UltrasoundAssistant.DoctorClient.Models.Enums;

namespace UltrasoundAssistant.DoctorClient.Models.Reads.Reports.Search;

/// <summary>
/// Краткая информация об отчёте.
/// </summary>
public sealed class ReportSummaryDto
{
    /// <summary>
    /// Идентификатор отчёта.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор записи на приём.
    /// </summary>
    public Guid AppointmentId { get; set; }

    /// <summary>
    /// ФИО пациента.
    /// </summary>
    public string PatientFullName { get; set; } = string.Empty;

    /// <summary>
    /// ФИО врача.
    /// </summary>
    public string DoctorFullName { get; set; } = string.Empty;

    /// <summary>
    /// Название шаблона.
    /// </summary>
    public string TemplateName { get; set; } = string.Empty;

    /// <summary>
    /// Дата начала приёма.
    /// </summary>
    public DateTime AppointmentStartAtUtc { get; set; }

    /// <summary>
    /// Статус отчёта.
    /// </summary>
    public ReportStatus Status { get; set; }

    /// <summary>
    /// Дата создания отчёта.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Версия агрегата.
    /// </summary>
    public int Version { get; set; }
}
