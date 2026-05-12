using UltrasoundAssistant.DoctorClient.Models.Statistics;

namespace UltrasoundAssistant.DoctorClient.ViewModels.Statistics;

public sealed class AppointmentStatusStatisticsViewItem
{
    public AppointmentStatusStatisticsDto Source { get; }

    public string StatusText =>
        !string.IsNullOrWhiteSpace(Source.StatusDisplayName)
            ? Source.StatusDisplayName
            : Source.Status;

    public int Count => Source.Count;

    public AppointmentStatusStatisticsViewItem(AppointmentStatusStatisticsDto source)
    {
        Source = source;
    }
}
