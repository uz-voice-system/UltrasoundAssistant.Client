namespace UltrasoundAssistant.DoctorClient.Models.Reads.Reports.Details;

public sealed class ReportPdfFileDto
{
    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = "application/pdf";

    public byte[] Content { get; set; } = [];
}
