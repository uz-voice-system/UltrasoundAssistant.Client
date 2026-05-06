using System.Text.Json.Serialization;

namespace UltrasoundAssistant.DoctorClient.Models.Entity.Templates;

/// <summary>
/// Тип значения поля шаблона
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TemplateFieldType
{
    /// <summary>
    /// Текстовое значение
    /// </summary>
    Text = 0,

    /// <summary>
    /// Числовое значение
    /// </summary>
    Number = 1,

    /// <summary>
    /// Числовое значение с единицей измерения
    /// </summary>
    NumberWithUnit = 2
}
