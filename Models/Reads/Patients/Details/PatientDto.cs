using UltrasoundAssistant.DoctorClient.Models.Entity.Patients;

namespace UltrasoundAssistant.DoctorClient.Models.Reads.Patients.Details;

/// <summary>
/// Полная информация о пациенте.
/// </summary>
public sealed class PatientDto
{
    /// <summary>
    /// Идентификатор пациента.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ФИО пациента.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Дата рождения.
    /// </summary>
    public DateTime BirthDate { get; set; }

    /// <summary>
    /// Пол.
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Номер телефона.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Электронная почта.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Признак удаления.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Версия агрегата.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Документы пациента.
    /// </summary>
    public List<PatientDocumentDto> Documents { get; set; } = [];
}
