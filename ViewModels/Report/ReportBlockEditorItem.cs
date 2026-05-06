using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace UltrasoundAssistant.DoctorClient.ViewModels.Report;

public partial class ReportBlockEditorItem : ObservableObject
{
    [ObservableProperty]
    private string blockName = string.Empty;

    [ObservableProperty]
    private string blockPhrasesText = string.Empty;

    public ObservableCollection<ReportFieldEditorItem> Fields { get; } = new();
}
