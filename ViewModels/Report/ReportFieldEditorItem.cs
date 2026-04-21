using CommunityToolkit.Mvvm.ComponentModel;

namespace UltrasoundAssistant.DoctorClient.ViewModels.Report;

public partial class ReportFieldEditorItem : ObservableObject
{
    [ObservableProperty]
    private string fieldName = string.Empty;

    [ObservableProperty]
    private string keyword = string.Empty;

    [ObservableProperty]
    private string value = string.Empty;

    [ObservableProperty]
    private string originalValue = string.Empty;

    public bool IsDirty =>
        !string.Equals(Value?.Trim(), OriginalValue?.Trim(), StringComparison.Ordinal);
}