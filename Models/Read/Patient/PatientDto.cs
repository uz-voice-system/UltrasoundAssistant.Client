namespace UltrasoundAssistant.DoctorClient.Models.Read.Patient;

/// <summary>
/// DTO пациента.
/// </summary>
public class PatientDto
{
    /// <summary>
    /// Идентификатор пациента.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор пациента для таблиц.
    /// </summary>
    public string ShortId => Id.ToString()[..8];

    /// <summary>
    /// Полное имя пациента.
    /// </summary>
    public string FullName { get; set; } = null!;

    /// <summary>
    /// Дата рождения пациента.
    /// </summary>
    public DateTime BirthDate { get; set; }

    /// <summary>
    /// Возраст пациента.
    /// </summary>
    public int Age => DateTime.Today.Year - BirthDate.Year;

    /// <summary>
    /// Пол пациента.
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Версия агрегата (для optimistic concurrency при обновлении/деактивации).
    /// </summary>
    public int Version { get; set; }
}