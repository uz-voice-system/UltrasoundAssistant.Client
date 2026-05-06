using System.Text.Json.Serialization;

namespace UltrasoundAssistant.DoctorClient.Models.Enums;

/// <summary>
/// Тип документа пациента.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PatientDocumentType
{
    /// <summary>
    /// Паспорт.
    /// </summary>
    Passport = 0,

    /// <summary>
    /// СНИЛС.
    /// </summary>
    Snils = 1,

    /// <summary>
    /// Полис ОМС.
    /// </summary>
    OmsPolicy = 2,

    /// <summary>
    /// Медицинская карта.
    /// </summary>
    MedicalCard = 3,

    /// <summary>
    /// Иной документ.
    /// </summary>
    Other = 4
}
