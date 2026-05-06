using UltrasoundAssistant.DoctorClient.Models.Enums;

namespace UltrasoundAssistant.DoctorClient.Models.Reads.Appointments.Search;

/// <summary>
/// Фильтр поиска записей на приём.
/// </summary>
public sealed class AppointmentSearchRequest
{
    /// <summary>
    /// Идентификатор пациента.
    /// </summary>
    public Guid? PatientId { get; set; }

    /// <summary>
    /// Идентификатор врача.
    /// </summary>
    public Guid? DoctorId { get; set; }

    /// <summary>
    /// Идентификатор шаблона исследования.
    /// </summary>
    public Guid? TemplateId { get; set; }

    /// <summary>
    /// Идентификатор пользователя, создавшего запись.
    /// </summary>
    public Guid? CreatedByUserId { get; set; }

    /// <summary>
    /// Статус записи.
    /// </summary>
    public AppointmentStatus? Status { get; set; }

    /// <summary>
    /// Начало периода поиска.
    /// </summary>
    public DateTime? FromUtc { get; set; }

    /// <summary>
    /// Окончание периода поиска.
    /// </summary>
    public DateTime? ToUtc { get; set; }

    /// <summary>
    /// Общая строка поиска.
    /// Используется для поиска по ФИО пациента, ФИО врача или названию шаблона.
    /// </summary>
    public string? SearchText { get; set; }

    /// <summary>
    /// Включать удалённые записи.
    /// </summary>
    public bool IncludeDeleted { get; set; }
}