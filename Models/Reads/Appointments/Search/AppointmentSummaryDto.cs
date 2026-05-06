using UltrasoundAssistant.DoctorClient.Models.Enums;

namespace UltrasoundAssistant.DoctorClient.Models.Reads.Appointments.Search;

/// <summary>
/// Краткая информация о записи на приём.
/// </summary>
public sealed class AppointmentSummaryDto
{
    /// <summary>
    /// Идентификатор записи.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор пациента.
    /// </summary>
    public Guid PatientId { get; set; }

    /// <summary>
    /// ФИО пациента.
    /// </summary>
    public string PatientFullName { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор врача.
    /// </summary>
    public Guid DoctorId { get; set; }

    /// <summary>
    /// ФИО врача.
    /// </summary>
    public string DoctorFullName { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор шаблона.
    /// </summary>
    public Guid TemplateId { get; set; }

    /// <summary>
    /// Название шаблона.
    /// </summary>
    public string TemplateName { get; set; } = string.Empty;

    /// <summary>
    /// Начало приёма.
    /// </summary>
    public DateTime StartAtUtc { get; set; }

    /// <summary>
    /// Окончание приёма.
    /// </summary>
    public DateTime EndAtUtc { get; set; }

    /// <summary>
    /// Статус записи.
    /// </summary>
    public AppointmentStatus Status { get; set; }

    /// <summary>
    /// Идентификатор отчёта, если он создан.
    /// </summary>
    public Guid? ReportId { get; set; }

    /// <summary>
    /// Версия агрегата.
    /// </summary>
    public int Version { get; set; }
}
