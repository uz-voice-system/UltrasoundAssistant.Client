namespace UltrasoundAssistant.DoctorClient.Models.Commands.Template;

public sealed class DeleteTemplateCommand
{
    public Guid CommandId { get; set; }
    public Guid TemplateId { get; set; }
    public int ExpectedVersion { get; set; }
}