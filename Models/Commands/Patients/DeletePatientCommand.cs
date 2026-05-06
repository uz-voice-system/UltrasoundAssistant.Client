namespace UltrasoundAssistant.DoctorClient.Models.Commands.Patients;

/// <summary>
/// Команда удаления пациента
/// </summary>
public sealed class DeletePatientCommand
{
    /// <summary>
    /// Идентификатор пациента
    /// </summary>
    public Guid PatientId { get; set; }

    /// <summary>
    /// Ожидаемая версия агрегата
    /// </summary>
    public int ExpectedVersion { get; set; }
}
