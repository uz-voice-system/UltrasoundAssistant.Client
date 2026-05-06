using UltrasoundAssistant.DoctorClient.Models.Enums;

namespace UltrasoundAssistant.DoctorClient.Models.Commands.Appointments;

/// <summary>
/// Команда обновления записи на приём
/// </summary>
public sealed class UpdateAppointmentCommand
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
    /// Начало приёма
    /// </summary>
    public DateTime StartAtUtc { get; set; }

    /// <summary>
    /// Окончание приёма
    /// </summary>
    public DateTime EndAtUtc { get; set; }

    /// <summary>
    /// Статус записи
    /// </summary>
    public AppointmentStatus Status { get; set; }

    /// <summary>
    /// Комментарий к записи
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Ожидаемая версия агрегата
    /// </summary>
    public int ExpectedVersion { get; set; }
}
