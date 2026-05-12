using CommunityToolkit.Mvvm.ComponentModel;
using UltrasoundAssistant.DoctorClient.Models.Entity.Templates;

namespace UltrasoundAssistant.DoctorClient.ViewModels.Template;

public partial class EditableTemplateFieldItem : ObservableObject
{
    [ObservableProperty]
    private Guid id;

    [ObservableProperty]
    private string fieldName = string.Empty;

    [ObservableProperty]
    private string displayName = string.Empty;

    [ObservableProperty]
    private int position;

    [ObservableProperty]
    private string phrasesText = string.Empty;

    [ObservableProperty]
    private TemplateFieldType type = TemplateFieldType.Text;

    [ObservableProperty]
    private TemplateFieldRole role = TemplateFieldRole.Regular;

    [ObservableProperty]
    private bool hasNorm;

    [ObservableProperty]
    private string normMin = string.Empty;

    [ObservableProperty]
    private string normMax = string.Empty;

    [ObservableProperty]
    private string normUnit = string.Empty;

    [ObservableProperty]
    private string normNormalText = string.Empty;
}
