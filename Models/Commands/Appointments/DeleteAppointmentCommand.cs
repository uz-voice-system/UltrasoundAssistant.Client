namespace UltrasoundAssistant.DoctorClient.Models.Commands.Appointments;

/// <summary>
/// Команда удаления записи на приём
/// </summary>
public sealed class DeleteAppointmentCommand
{
    /// <summary>
    /// Идентификатор записи
    /// </summary>
    public Guid AppointmentId { get; set; }

    /// <summary>
    /// Ожидаемая версия агрегата
    /// </summary>
    public int ExpectedVersion { get; set; }
}
