using Avalonia.Media;
using UltrasoundAssistant.DoctorClient.Models.Enums;
using UltrasoundAssistant.DoctorClient.Models.Reads.Appointments.Search;

namespace UltrasoundAssistant.DoctorClient.ViewModels.Appointment;

public sealed class AppointmentCardViewItem
{
    public AppointmentSummaryDto Source { get; }

    public Guid Id => Source.Id;
    public int Version => Source.Version;

    public string PatientFullName => Source.PatientFullName;
    public string DoctorFullName => Source.DoctorFullName;
    public string TemplateName => Source.TemplateName;

    public AppointmentStatus Status => Source.Status;

    public DateTime StartLocal => ToLocalFromUtc(Source.StartAtUtc);
    public DateTime EndLocal => ToLocalFromUtc(Source.EndAtUtc);

    public string DateText => StartLocal.ToString("dd.MM.yyyy");
    public string TimeText => $"{StartLocal:HH:mm}–{EndLocal:HH:mm}";

    public string StatusText => AppointmentRegistrationViewModel.LocalizeStatus(Status);

    public string ReportText => Source.ReportId == null
        ? "Отчёт не создан"
        : "Отчёт создан";

    public bool CanCancel => Status == AppointmentStatus.Scheduled;

    public IBrush CardBackground => Status switch
    {
        AppointmentStatus.Canceled => Brush.Parse("#FFF5F5"),
        AppointmentStatus.NoShow => Brush.Parse("#FFF3CD"),
        AppointmentStatus.Completed => Brush.Parse("#F7F8FA"),
        AppointmentStatus.InProgress => Brush.Parse("#E8F1FF"),
        _ => Brushes.White
    };

    public IBrush CardBorderBrush => Status switch
    {
        AppointmentStatus.Canceled => Brush.Parse("#E74C3C"),
        AppointmentStatus.NoShow => Brush.Parse("#D99A00"),
        AppointmentStatus.InProgress => Brush.Parse("#2F80ED"),
        _ => Brush.Parse("#DDDDDD")
    };

    public IBrush StatusBackground => Status switch
    {
        AppointmentStatus.Scheduled => Brush.Parse("#E8F1FF"),
        AppointmentStatus.InProgress => Brush.Parse("#FFF3CD"),
        AppointmentStatus.Completed => Brush.Parse("#DFF5E1"),
        AppointmentStatus.Canceled => Brush.Parse("#FDE2E2"),
        AppointmentStatus.NoShow => Brush.Parse("#FFE8CC"),
        _ => Brush.Parse("#EEF2F7")
    };

    public IBrush StatusForeground => Status switch
    {
        AppointmentStatus.Scheduled => Brush.Parse("#2457A6"),
        AppointmentStatus.InProgress => Brush.Parse("#7A5B00"),
        AppointmentStatus.Completed => Brush.Parse("#1E7E34"),
        AppointmentStatus.Canceled => Brush.Parse("#B00020"),
        AppointmentStatus.NoShow => Brush.Parse("#A15C00"),
        _ => Brush.Parse("#444444")
    };

    public AppointmentCardViewItem(AppointmentSummaryDto source)
    {
        Source = source;
    }

    private static DateTime ToLocalFromUtc(DateTime utcDateTime)
    {
        var utc = utcDateTime.Kind == DateTimeKind.Utc
            ? utcDateTime
            : DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

        return utc.ToLocalTime();
    }
}
