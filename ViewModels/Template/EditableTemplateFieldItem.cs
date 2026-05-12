using CommunityToolkit.Mvvm.ComponentModel;

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
    private string typeText = "Текст";

    [ObservableProperty]
    private string roleText = "Обычное поле";

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
