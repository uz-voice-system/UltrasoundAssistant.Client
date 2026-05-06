using CommunityToolkit.Mvvm.ComponentModel;

namespace UltrasoundAssistant.DoctorClient.ViewModels.Report;

public partial class ReportFieldEditorItem : ObservableObject
{
    [ObservableProperty]
    private string blockName = string.Empty;

    [ObservableProperty]
    private string fieldName = string.Empty;

    [ObservableProperty]
    private string displayName = string.Empty;

    [ObservableProperty]
    private string phrasesText = string.Empty;

    [ObservableProperty]
    private string value = string.Empty;

    [ObservableProperty]
    private string originalValue = string.Empty;

    [ObservableProperty]
    private string? normMessage;

    public bool IsDirty =>
        !string.Equals(Value?.Trim(), OriginalValue?.Trim(), StringComparison.Ordinal);
}
