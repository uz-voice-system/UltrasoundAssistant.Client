using UltrasoundAssistant.DoctorClient.Models.Enums;

namespace UltrasoundAssistant.DoctorClient.Models.Reads.Patients.Search;

/// <summary>
/// Фильтр поиска пациентов.
/// </summary>
public sealed class PatientSearchRequest
{
    /// <summary>
    /// Общая строка поиска.
    /// Используется для поиска по ФИО, телефону, почте или номеру документа.
    /// </summary>
    public string? SearchText { get; set; }

    /// <summary>
    /// ФИО пациента.
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Дата рождения.
    /// </summary>
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// Тип документа.
    /// </summary>
    public PatientDocumentType? DocumentType { get; set; }

    /// <summary>
    /// Серия документа.
    /// </summary>
    public string? DocumentSeries { get; set; }

    /// <summary>
    /// Номер документа.
    /// </summary>
    public string? DocumentNumber { get; set; }

    /// <summary>
    /// Телефон пациента.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Включать удалённых пациентов.
    /// </summary>
    public bool IncludeDeleted { get; set; }
}
