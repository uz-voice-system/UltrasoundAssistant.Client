using Avalonia.Media;
using System.Collections.ObjectModel;

namespace UltrasoundAssistant.DoctorClient.ViewModels.Appointment;

public sealed class DoctorDayScheduleViewItem
{
    public DateTime Date { get; set; }

    public string DayName { get; set; } = string.Empty;

    public string DateText { get; set; } = string.Empty;

    public string StatusText { get; set; } = string.Empty;

    public bool IsSelectedDay { get; set; }

    public IBrush StatusBackground { get; set; } = Brush.Parse("#EEF2F7");

    public IBrush StatusForeground { get; set; } = Brush.Parse("#444444");

    public ObservableCollection<DoctorTimeSlotViewItem> Slots { get; } = new();

    public IBrush CardBackground => IsSelectedDay
        ? Brush.Parse("#FFF8E1")
        : Brush.Parse("#F7F8FA");

    public IBrush CardBorderBrush => IsSelectedDay
        ? Brush.Parse("#F2C94C")
        : Brush.Parse("#DDDDDD");
}
