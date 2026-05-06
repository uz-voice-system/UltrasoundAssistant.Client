namespace UltrasoundAssistant.DoctorClient.Models.Reads.Patients.Search;

/// <summary>
/// Краткая информация о пациенте для списков и поиска.
/// </summary>
public sealed class PatientSummaryDto
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
    /// Телефон.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Признак удаления.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Версия агрегата.
    /// </summary>
    public int Version { get; set; }
}
