using UltrasoundAssistant.DoctorClient.Models.Statistics;

namespace UltrasoundAssistant.DoctorClient.ViewModels.Statistics;

public sealed class ReportStatusStatisticsViewItem
{
    public ReportStatusStatisticsDto Source { get; }

    public string StatusText =>
        !string.IsNullOrWhiteSpace(Source.StatusDisplayName)
            ? Source.StatusDisplayName
            : Source.Status;

    public int Count => Source.Count;

    public ReportStatusStatisticsViewItem(ReportStatusStatisticsDto source)
    {
        Source = source;
    }
}
