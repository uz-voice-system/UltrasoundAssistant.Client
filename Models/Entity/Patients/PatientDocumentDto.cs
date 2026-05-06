using UltrasoundAssistant.DoctorClient.Models.Enums;

namespace UltrasoundAssistant.DoctorClient.Models.Entity.Patients;

/// <summary>
/// Документ пациента.
/// </summary>
public sealed class PatientDocumentDto
{
    /// <summary>
    /// Идентификатор документа.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Тип документа.
    /// </summary>
    public PatientDocumentType DocumentType { get; set; }

    /// <summary>
    /// Серия документа.
    /// </summary>
    public string? Series { get; set; }

    /// <summary>
    /// Номер документа.
    /// </summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>
    /// Кем выдан документ.
    /// </summary>
    public string? IssuedBy { get; set; }

    /// <summary>
    /// Дата выдачи документа.
    /// </summary>
    public DateTime? IssueDate { get; set; }

    /// <summary>
    /// Код подразделения.
    /// </summary>
    public string? DepartmentCode { get; set; }

    /// <summary>
    /// Организация, связанная с документом.
    /// Например, страховая компания для полиса ОМС.
    /// </summary>
    public string? Organization { get; set; }
}
