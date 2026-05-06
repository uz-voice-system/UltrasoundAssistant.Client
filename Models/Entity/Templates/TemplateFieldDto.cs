namespace UltrasoundAssistant.DoctorClient.Models.Entity.Templates;

/// <summary>
/// Поле блока шаблона
/// </summary>
public sealed class TemplateFieldDto
{
    /// <summary>
    /// Идентификатор поля
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Техническое имя поля
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Отображаемое название поля
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Порядок отображения поля
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Фразы для определения поля
    /// </summary>
    public List<string> Phrases { get; set; } = [];

    /// <summary>
    /// Тип значения поля
    /// </summary>
    public TemplateFieldType Type { get; set; } = TemplateFieldType.Text;

    /// <summary>
    /// Роль поля в шаблоне.
    /// </summary>
    public TemplateFieldRole Role { get; set; } = TemplateFieldRole.Regular;

    /// <summary>
    /// Норма значения поля
    /// </summary>
    public FieldNormDto? Norm { get; set; }
}
