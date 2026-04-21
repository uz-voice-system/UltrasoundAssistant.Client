namespace UltrasoundAssistant.DoctorClient.Models.Commands.Report;

public sealed class DeleteReportCommand
{
    public Guid CommandId { get; set; }
    public Guid ReportId { get; set; }
    public int ExpectedVersion { get; set; }
}