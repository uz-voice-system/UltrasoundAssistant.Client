using UltrasoundAssistant.DoctorClient.Models.Entity.Templates;

namespace UltrasoundAssistant.DoctorClient.Models.Reads.Templates.Admin;

/// <summary>
/// Расширенный фильтр поиска шаблонов
/// </summary>
public sealed class TemplateAdminSearchRequest
{
    /// <summary>
    /// Общая строка поиска
    /// </summary>
    public string? SearchText { get; set; }

    /// <summary>
    /// Название шаблона
    /// </summary>
    public string? TemplateName { get; set; }

    /// <summary>
    /// Название блока
    /// </summary>
    public string? BlockName { get; set; }

    /// <summary>
    /// Техническое имя поля
    /// </summary>
    public string? FieldName { get; set; }

    /// <summary>
    /// Отображаемое название поля
    /// </summary>
    public string? FieldDisplayName { get; set; }

    /// <summary>
    /// Фраза блока или поля
    /// </summary>
    public string? Phrase { get; set; }

    /// <summary>
    /// Тип поля
    /// </summary>
    public TemplateFieldType? FieldType { get; set; }

    /// <summary>
    /// Роль поля.
    /// </summary>
    public TemplateFieldRole? FieldRole { get; set; }

    /// <summary>
    /// Признак наличия нормы
    /// </summary>
    public bool? HasNorm { get; set; }

    /// <summary>
    /// Включать удалённые шаблоны
    /// </summary>
    public bool IncludeDeleted { get; set; }
}
