using Avalonia.Media;

namespace UltrasoundAssistant.DoctorClient.ViewModels.Appointment;

public sealed class DoctorAvailabilityDayItem
{
    public DateTime Date { get; init; }

    public string DayTitle => Date.ToString("dddd, dd.MM.yyyy");

    public bool IsWorkingDay { get; init; }

    public bool IsSelectedDate { get; init; }

    public TimeSpan? WorkStart { get; init; }

    public TimeSpan? WorkEnd { get; init; }

    public bool? IsFreeForSelectedTime { get; init; }

    public string BusyIntervalsText { get; init; } = string.Empty;

    public string WorkTimeText
    {
        get
        {
            if (!IsWorkingDay || WorkStart == null || WorkEnd == null)
                return "Нерабочий день";

            return $"Рабочее время: {WorkStart:hh\\:mm} — {WorkEnd:hh\\:mm}";
        }
    }

    public string BusyText => string.IsNullOrWhiteSpace(BusyIntervalsText)
        ? "Занятых интервалов нет"
        : $"Занято: {BusyIntervalsText}";

    public string StatusText
    {
        get
        {
            if (!IsWorkingDay)
                return "Нерабочий день";

            if (!IsSelectedDate)
                return "Рабочий день";

            return IsFreeForSelectedTime switch
            {
                true => "Свободен на выбранное время",
                false => "Занят на выбранное время",
                _ => "Выберите время"
            };
        }
    }

    public IBrush CardBackground
    {
        get
        {
            if (!IsWorkingDay)
                return Brush.Parse("#F5F5F5");

            if (!IsSelectedDate)
                return Brushes.White;

            return IsFreeForSelectedTime == true
                ? Brush.Parse("#F0FFF4")
                : Brush.Parse("#FFF5F5");
        }
    }

    public IBrush CardBorderBrush
    {
        get
        {
            if (!IsWorkingDay)
                return Brush.Parse("#DDDDDD");

            if (!IsSelectedDate)
                return Brush.Parse("#DDDDDD");

            return IsFreeForSelectedTime == true
                ? Brush.Parse("#2ECC71")
                : Brush.Parse("#E74C3C");
        }
    }

    public IBrush StatusBackground
    {
        get
        {
            if (!IsWorkingDay)
                return Brush.Parse("#EEF2F7");

            if (!IsSelectedDate)
                return Brush.Parse("#E8F1FF");

            return IsFreeForSelectedTime == true
                ? Brush.Parse("#DFF5E1")
                : Brush.Parse("#FDE2E2");
        }
    }

    public IBrush StatusForeground
    {
        get
        {
            if (!IsWorkingDay)
                return Brush.Parse("#555555");

            if (!IsSelectedDate)
                return Brush.Parse("#2457A6");

            return IsFreeForSelectedTime == true
                ? Brush.Parse("#1E7E34")
                : Brush.Parse("#B00020");
        }
    }
}
