using System.Text.Json.Serialization;

namespace UltrasoundAssistant.DoctorClient.Models.Voice;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NormStatus
{
    /// <summary>
    /// Норма не задана
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Значение соответствует норме
    /// </summary>
    Normal = 1,

    /// <summary>
    /// Значение ниже нормы
    /// </summary>
    Below = 2,

    /// <summary>
    /// Значение выше нормы
    /// </summary>
    Above = 3,

    /// <summary>
    /// Текстовое значение отличается от нормы
    /// </summary>
    AbnormalText = 4
}
