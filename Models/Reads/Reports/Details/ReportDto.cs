using UltrasoundAssistant.DoctorClient.Models.Enums;

namespace UltrasoundAssistant.DoctorClient.Models.Reads.Reports.Details;

/// <summary>
/// Полная информация об отчёте.
/// </summary>
public sealed class ReportDto
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
    /// Идентификатор пациента.
    /// Получается через запись на приём.
    /// </summary>
    public Guid PatientId { get; set; }

    /// <summary>
    /// ФИО пациента.
    /// </summary>
    public string PatientFullName { get; set; } = string.Empty;

    /// <summary>
    /// Дата рождения пациента.
    /// </summary>
    public DateTime? PatientBirthDate { get; set; }

    /// <summary>
    /// Пол пациента.
    /// </summary>
    public string? PatientGender { get; set; }

    /// <summary>
    /// Идентификатор врача.
    /// Получается через запись на приём.
    /// </summary>
    public Guid DoctorId { get; set; }

    /// <summary>
    /// ФИО врача.
    /// </summary>
    public string DoctorFullName { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор шаблона.
    /// Получается через запись на приём.
    /// </summary>
    public Guid TemplateId { get; set; }

    /// <summary>
    /// Название шаблона.
    /// </summary>
    public string TemplateName { get; set; } = string.Empty;

    /// <summary>
    /// Дата и время начала приёма.
    /// </summary>
    public DateTime? AppointmentStartAtUtc { get; set; }

    /// <summary>
    /// Дата и время окончания приёма.
    /// </summary>
    public DateTime? AppointmentEndAtUtc { get; set; }

    /// <summary>
    /// Статус отчёта.
    /// </summary>
    public ReportStatus Status { get; set; }

    /// <summary>
    /// Содержимое отчёта в формате JSON.
    /// </summary>
    public string ContentJson { get; set; } = "{}";

    /// <summary>
    /// Признак удаления.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Дата создания.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Дата обновления.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }

    /// <summary>
    /// Версия агрегата.
    /// </summary>
    public int Version { get; set; }
}
