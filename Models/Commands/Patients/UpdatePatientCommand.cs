using UltrasoundAssistant.DoctorClient.Models.Entity.Patients;

namespace UltrasoundAssistant.DoctorClient.Models.Commands.Patients;

/// <summary>
/// Команда обновления пациента
/// </summary>
public sealed class UpdatePatientCommand
{
    /// <summary>
    /// Идентификатор пациента
    /// </summary>
    public Guid PatientId { get; set; }

    /// <summary>
    /// ФИО пациента
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Дата рождения
    /// </summary>
    public DateTime BirthDate { get; set; }

    /// <summary>
    /// Пол
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Номер телефона
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Электронная почта
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Документы пациента
    /// </summary>
    public List<PatientDocumentDto> Documents { get; set; } = [];

    /// <summary>
    /// Ожидаемая версия агрегата
    /// </summary>
    public int ExpectedVersion { get; set; }
}
