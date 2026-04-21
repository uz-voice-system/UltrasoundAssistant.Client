namespace UltrasoundAssistant.DoctorClient.Models.Read.Template;

/// <summary>
/// DTO шаблона отчёта.
/// </summary>
public class TemplateDto
{
    /// <summary>
    /// Идентификатор шаблона.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор шаблона для таблиц.
    /// </summary>
    public string ShortId => Id.ToString()[..8];

    /// <summary>
    /// Название шаблона.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Ключевые слова шаблона.
    /// </summary>
    public List<TemplateKeywordDto> Keywords { get; set; } = [];

    /// <summary>
    /// Версия агрегата (для команд обновления и удаления).
    /// </summary>
    public int Version { get; set; }
}