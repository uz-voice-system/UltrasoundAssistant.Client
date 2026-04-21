namespace UltrasoundAssistant.DoctorClient.Models.Commands.Report;

public sealed class CreateReportCommand
{
    public Guid CommandId { get; set; }
    public Guid ReportId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid TemplateId { get; set; }
}