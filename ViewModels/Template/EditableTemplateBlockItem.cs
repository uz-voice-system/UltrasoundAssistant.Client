using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace UltrasoundAssistant.DoctorClient.ViewModels.Template;

public partial class EditableTemplateBlockItem : ObservableObject
{
    [ObservableProperty]
    private Guid id;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private int position;

    [ObservableProperty]
    private string phrasesText = string.Empty;

    [ObservableProperty]
    private string defaultFieldName = string.Empty;

    public ObservableCollection<EditableTemplateFieldItem> Fields { get; } = new();
}
