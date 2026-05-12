using CommunityToolkit.Mvvm.ComponentModel;

namespace UltrasoundAssistant.DoctorClient.ViewModels.User;

public partial class EditableUserScheduleItem : ObservableObject
{
    [ObservableProperty]
    private Guid scheduleId;

    [ObservableProperty]
    private bool isEnabled;

    [ObservableProperty]
    private DayOfWeek dayOfWeek;

    [ObservableProperty]
    private string dayName = string.Empty;

    [ObservableProperty]
    private TimeSpan? startTime = new(9, 0, 0);

    [ObservableProperty]
    private TimeSpan? endTime = new(18, 0, 0);
}
