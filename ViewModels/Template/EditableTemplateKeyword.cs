namespace UltrasoundAssistant.DoctorClient.ViewModels.Template;

public class EditableTemplateKeyword : ViewModelBase
{
    private string _phrase = string.Empty;
    public string Phrase
    {
        get => _phrase;
        set => SetProperty(ref _phrase, value);
    }

    private string _fieldName = string.Empty;
    public string FieldName
    {
        get => _fieldName;
        set => SetProperty(ref _fieldName, value);
    }
}