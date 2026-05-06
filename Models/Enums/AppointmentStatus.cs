using System.Text.Json.Serialization;

namespace UltrasoundAssistant.DoctorClient.Models.Enums;

/// <summary>
/// Статус записи на приём.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AppointmentStatus
{
    /// <summary>
    /// Запланирована.
    /// </summary>
    Scheduled = 0,

    /// <summary>
    /// Приём начат.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Приём завершён.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Запись отменена.
    /// </summary>
    Canceled = 3,

    /// <summary>
    /// Пациент не явился.
    /// </summary>
    NoShow = 4
}
