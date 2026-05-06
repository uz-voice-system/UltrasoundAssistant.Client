using UltrasoundAssistant.DoctorClient.Models.Reads.Templates.Details;

namespace UltrasoundAssistant.DoctorClient.Models.Reads.Templates.Admin;

/// <summary>
/// Результат расширенного поиска шаблона
/// </summary>
public sealed class TemplateAdminSearchResultDto
{
    /// <summary>
    /// Шаблон отчёта
    /// </summary>
    public TemplateDto Template { get; set; } = new();

    /// <summary>
    /// Совпадения внутри шаблона
    /// </summary>
    public List<TemplateSearchMatchDto> Matches { get; set; } = [];
}
