namespace UltrasoundAssistant.DoctorClient.Models.Commands.Templates;

/// <summary>
/// Команда удаления шаблона
/// </summary>
public sealed class DeleteTemplateCommand
{
    /// <summary>
    /// Идентификатор шаблона
    /// </summary>
    public Guid TemplateId { get; set; }

    /// <summary>
    /// Ожидаемая версия агрегата
    /// </summary>
    public int ExpectedVersion { get; set; }
}
