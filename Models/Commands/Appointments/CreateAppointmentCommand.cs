namespace UltrasoundAssistant.DoctorClient.Models.Commands.Appointments;

/// <summary>
/// Команда создания записи на приём
/// </summary>
public sealed class CreateAppointmentCommand
{
    /// <summary>
    /// Идентификатор записи
    /// </summary>
    public Guid AppointmentId { get; set; }

    /// <summary>
    /// Идентификатор пациента
    /// </summary>
    public Guid PatientId { get; set; }

    /// <summary>
    /// Идентификатор врача
    /// </summary>
    public Guid DoctorId { get; set; }

    /// <summary>
    /// Идентификатор шаблона исследования
    /// </summary>
    public Guid TemplateId { get; set; }

    /// <summary>
    /// Идентификатор пользователя, создавшего запись
    /// </summary>
    public Guid CreatedByUserId { get; set; }

    /// <summary>
    /// Начало приёма
    /// </summary>
    public DateTime StartAtUtc { get; set; }

    /// <summary>
    /// Окончание приёма
    /// </summary>
    public DateTime EndAtUtc { get; set; }

    /// <summary>
    /// Комментарий к записи
    /// </summary>
    public string? Comment { get; set; }
}
