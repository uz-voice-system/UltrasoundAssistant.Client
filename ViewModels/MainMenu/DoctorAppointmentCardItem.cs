using Avalonia.Media;
using UltrasoundAssistant.DoctorClient.Models.Enums;
using UltrasoundAssistant.DoctorClient.Models.Reads.Appointments.Search;

namespace UltrasoundAssistant.DoctorClient.ViewModels.MainMenu;

public sealed class DoctorAppointmentCardItem
{
    public AppointmentSummaryDto Source { get; }

    public Guid Id => Source.Id;
    public Guid? ReportId => Source.ReportId;

    public string PatientFullName => Source.PatientFullName;
    public string DoctorFullName => Source.DoctorFullName;
    public string TemplateName => Source.TemplateName;

    public AppointmentStatus Status => Source.Status;
    public int Version => Source.Version;

    public DateTime LocalStart => Source.StartAtUtc.Kind == DateTimeKind.Utc
        ? Source.StartAtUtc.ToLocalTime()
        : DateTime.SpecifyKind(Source.StartAtUtc, DateTimeKind.Utc).ToLocalTime();

    public DateTime LocalEnd => Source.EndAtUtc.Kind == DateTimeKind.Utc
        ? Source.EndAtUtc.ToLocalTime()
        : DateTime.SpecifyKind(Source.EndAtUtc, DateTimeKind.Utc).ToLocalTime();

    public string DateText => LocalStart.ToString("dd.MM.yyyy");
    public string TimeText => $"{LocalStart:HH:mm}–{LocalEnd:HH:mm}";

    public string StatusText => Status switch
    {
        AppointmentStatus.Scheduled => "Запланирована",
        AppointmentStatus.InProgress => "В процессе",
        AppointmentStatus.Completed => "Завершена",
        AppointmentStatus.Canceled => "Отменена",
        AppointmentStatus.NoShow => "Неявка",
        _ => Status.ToString()
    };

    public string ReportText => ReportId == null
        ? "Отчёт не найден"
        : "Отчёт создан";

    public bool CanOpenReport => Status is AppointmentStatus.Scheduled or AppointmentStatus.InProgress or AppointmentStatus.Completed;
    public bool CanMarkNoShow => Status is AppointmentStatus.Scheduled or AppointmentStatus.InProgress;

    public IBrush CardBackground => Status switch
    {
        AppointmentStatus.Canceled => Brush.Parse("#FFF5F5"),
        AppointmentStatus.NoShow => Brush.Parse("#FFF7E6"),
        AppointmentStatus.Completed => Brush.Parse("#F0FFF4"),
        AppointmentStatus.InProgress => Brush.Parse("#F3E8FF"),
        _ => Brushes.White
    };

    public IBrush CardBorderBrush => Status switch
    {
        AppointmentStatus.Canceled => Brush.Parse("#E74C3C"),
        AppointmentStatus.NoShow => Brush.Parse("#F39C12"),
        AppointmentStatus.Completed => Brush.Parse("#2ECC71"),
        AppointmentStatus.InProgress => Brush.Parse("#9B59B6"),
        _ => Brush.Parse("#DDDDDD")
    };

    public IBrush StatusBackground => Status switch
    {
        AppointmentStatus.Scheduled => Brush.Parse("#E8F1FF"),
        AppointmentStatus.InProgress => Brush.Parse("#F3E8FF"),
        AppointmentStatus.Completed => Brush.Parse("#DFF5E1"),
        AppointmentStatus.Canceled => Brush.Parse("#FDE2E2"),
        AppointmentStatus.NoShow => Brush.Parse("#FFF3CD"),
        _ => Brush.Parse("#EEF2F7")
    };

    public IBrush StatusForeground => Status switch
    {
        AppointmentStatus.Scheduled => Brush.Parse("#2457A6"),
        AppointmentStatus.InProgress => Brush.Parse("#6F2DA8"),
        AppointmentStatus.Completed => Brush.Parse("#1E7E34"),
        AppointmentStatus.Canceled => Brush.Parse("#B00020"),
        AppointmentStatus.NoShow => Brush.Parse("#7A5B00"),
        _ => Brush.Parse("#444444")
    };

    public DoctorAppointmentCardItem(AppointmentSummaryDto source)
    {
        Source = source;
    }
}
