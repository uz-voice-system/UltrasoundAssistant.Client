namespace UltrasoundAssistant.DoctorClient.Models.Commands.Report;

public sealed class UpdateReportFieldCommand
{
    public Guid CommandId { get; set; }
    public Guid ReportId { get; set; }
    public int ExpectedVersion { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public double Confidence { get; set; } = 1.0;
}