using Avalonia.Media;

namespace UltrasoundAssistant.DoctorClient.ViewModels.Appointment;

public sealed class DoctorTimeSlotViewItem
{
    public DateTime StartLocal { get; set; }

    public DateTime EndLocal { get; set; }

    public string TimeText { get; set; } = string.Empty;

    public bool IsBusy { get; set; }

    public bool IsPast { get; set; }

    public bool IsSelected { get; set; }

    public string PatientText { get; set; } = string.Empty;

    public bool CanSelect => !IsBusy && !IsPast;

    public string StatusText
    {
        get
        {
            if (IsPast)
                return "Прошло";

            return IsBusy ? "Занято" : "Свободно";
        }
    }

    public IBrush Background
    {
        get
        {
            if (IsSelected)
                return Brush.Parse("#FFF3CD");

            if (IsPast)
                return Brush.Parse("#F0F0F0");

            return IsBusy
                ? Brush.Parse("#FDE2E2")
                : Brush.Parse("#DFF5E1");
        }
    }

    public IBrush BorderBrush
    {
        get
        {
            if (IsSelected)
                return Brush.Parse("#D99A00");

            if (IsBusy)
                return Brush.Parse("#E74C3C");

            if (IsPast)
                return Brush.Parse("#CCCCCC");

            return Brush.Parse("#2ECC71");
        }
    }

    public IBrush Foreground
    {
        get
        {
            if (IsPast)
                return Brush.Parse("#777777");

            return IsBusy
                ? Brush.Parse("#B00020")
                : Brush.Parse("#1E7E34");
        }
    }
}
