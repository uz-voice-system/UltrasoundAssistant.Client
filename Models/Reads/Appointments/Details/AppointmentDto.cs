using UltrasoundAssistant.DoctorClient.Models.Enums;

namespace UltrasoundAssistant.DoctorClient.Models.Reads.Appointments.Details;

/// <summary>
/// Полная информация о записи на приём.
/// </summary>
public sealed class AppointmentDto
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
    /// Идентификатор шаблона исследования.
    /// </summary>
    public Guid TemplateId { get; set; }

    /// <summary>
    /// Название шаблона исследования.
    /// </summary>
    public string TemplateName { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор пользователя, создавшего запись.
    /// </summary>
    public Guid CreatedByUserId { get; set; }

    /// <summary>
    /// ФИО пользователя, создавшего запись.
    /// </summary>
    public string CreatedByUserFullName { get; set; } = string.Empty;

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
    /// Комментарий к записи.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Идентификатор отчёта, если он уже создан.
    /// </summary>
    public Guid? ReportId { get; set; }

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
