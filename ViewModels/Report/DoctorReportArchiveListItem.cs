using Avalonia.Media;
using UltrasoundAssistant.DoctorClient.Models.Enums;
using UltrasoundAssistant.DoctorClient.Models.Reads.Reports.Search;

namespace UltrasoundAssistant.DoctorClient.ViewModels.Report;

public sealed class DoctorReportArchiveListItem
{
    public ReportSummaryDto Source { get; }

    public Guid Id => Source.Id;
    public Guid AppointmentId => Source.AppointmentId;

    public string PatientFullName => string.IsNullOrWhiteSpace(Source.PatientFullName)
        ? "Пациент не указан"
        : Source.PatientFullName;

    public string DoctorFullName => string.IsNullOrWhiteSpace(Source.DoctorFullName)
        ? "Врач не указан"
        : Source.DoctorFullName;

    public string TemplateName => string.IsNullOrWhiteSpace(Source.TemplateName)
        ? "Шаблон не указан"
        : Source.TemplateName;

    public ReportStatus Status => Source.Status;

    public DateTime AppointmentStartAtUtc => Source.AppointmentStartAtUtc;
    public DateTime CreatedAtUtc => Source.CreatedAtUtc;
    public int Version => Source.Version;

    public bool CanEdit => Status is ReportStatus.Draft or ReportStatus.InProgress;
    public bool CanPrint => Status is ReportStatus.Completed or ReportStatus.Archived;
    public bool CanArchive => Status == ReportStatus.Completed;

    public string AppointmentTimeText
    {
        get
        {
            var local = ToLocalTime(AppointmentStartAtUtc);
            return local == DateTime.MinValue
                ? "Время приёма не указано"
                : local.ToString("dd.MM.yyyy HH:mm");
        }
    }

    public string CreatedAtText
    {
        get
        {
            var local = ToLocalTime(CreatedAtUtc);
            return local == DateTime.MinValue
                ? "Дата создания не указана"
                : local.ToString("dd.MM.yyyy HH:mm");
        }
    }

    public string StatusText => Status switch
    {
        ReportStatus.Draft => "Черновик",
        ReportStatus.InProgress => "В процессе",
        ReportStatus.Completed => "Завершён",
        ReportStatus.Archived => "Архивирован",
        _ => Status.ToString()
    };

    public string EditButtonText => CanEdit ? "Редактировать" : "Редактирование недоступно";
    public string PrintButtonText => CanPrint ? "Печать" : "Печать недоступна";

    public IBrush CardBackground => Status switch
    {
        ReportStatus.Draft => Hex("#FFFFFF"),
        ReportStatus.InProgress => Hex("#FFF8E1"),
        ReportStatus.Completed => Hex("#EAF7EA"),
        ReportStatus.Archived => Hex("#EEF2F7"),
        _ => Brushes.White
    };

    public IBrush CardBorderBrush => Status switch
    {
        ReportStatus.Draft => Hex("#DDDDDD"),
        ReportStatus.InProgress => Hex("#F2C94C"),
        ReportStatus.Completed => Hex("#2ECC71"),
        ReportStatus.Archived => Hex("#AAB2BD"),
        _ => Hex("#DDDDDD")
    };

    public IBrush StatusBackground => Status switch
    {
        ReportStatus.Draft => Hex("#EEF2F7"),
        ReportStatus.InProgress => Hex("#FFF3CD"),
        ReportStatus.Completed => Hex("#DFF5E1"),
        ReportStatus.Archived => Hex("#E8F1FF"),
        _ => Hex("#EEF2F7")
    };

    public IBrush StatusForeground => Status switch
    {
        ReportStatus.Draft => Hex("#444444"),
        ReportStatus.InProgress => Hex("#7A5B00"),
        ReportStatus.Completed => Hex("#1E7E34"),
        ReportStatus.Archived => Hex("#2457A6"),
        _ => Hex("#444444")
    };

    private static DateTime ToLocalTime(DateTime value)
    {
        if (value == DateTime.MinValue)
            return value;

        return value.Kind switch
        {
            DateTimeKind.Utc => value.ToLocalTime(),
            DateTimeKind.Local => value,
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc).ToLocalTime()
        };
    }

    private static IBrush Hex(string color)
    {
        return new SolidColorBrush(Color.Parse(color));
    }

    public DoctorReportArchiveListItem(ReportSummaryDto source)
    {
        Source = source;
    }
}
