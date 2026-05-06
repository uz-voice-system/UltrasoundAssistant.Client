namespace UltrasoundAssistant.DoctorClient.Models.Reads.Templates.Search;

/// <summary>
/// Фильтр поиска шаблонов для выбора
/// </summary>
public sealed class TemplateSearchRequest
{
    /// <summary>
    /// Строка поиска по названию шаблона
    /// </summary>
    public string? SearchText { get; set; }

    /// <summary>
    /// Включать удалённые шаблоны.
    /// </summary>
    public bool IncludeDeleted { get; set; }
}
