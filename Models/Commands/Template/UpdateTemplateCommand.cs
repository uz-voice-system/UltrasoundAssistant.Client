namespace UltrasoundAssistant.DoctorClient.Models.Commands.Template;

public sealed class UpdateTemplateCommand
{
    public Guid CommandId { get; set; }
    public Guid TemplateId { get; set; }
    public int ExpectedVersion { get; set; }
    public string? Name { get; set; }
    public Dictionary<string, string>? Keywords { get; set; }
}