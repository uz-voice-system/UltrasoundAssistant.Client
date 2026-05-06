using System.Text.Json.Serialization;

namespace UltrasoundAssistant.DoctorClient.Models.Entity.Templates;

/// <summary>
/// Роль поля шаблона.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TemplateFieldRole
{
    /// <summary>
    /// Обычное поле шаблона.
    /// </summary>
    Regular = 0,

    /// <summary>
    /// Поле описания исследования.
    /// Если поле пустое, значение может быть сгенерировано модулем формирования отчёта.
    /// </summary>
    Description = 1,

    /// <summary>
    /// Поле заключения исследования.
    /// Если поле пустое, значение может быть сгенерировано модулем формирования отчёта.
    /// </summary>
    Conclusion = 2
}
