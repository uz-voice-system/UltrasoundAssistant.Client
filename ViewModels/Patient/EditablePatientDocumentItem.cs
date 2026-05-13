using CommunityToolkit.Mvvm.ComponentModel;

namespace UltrasoundAssistant.DoctorClient.ViewModels.Patient;

public partial class EditablePatientDocumentItem : ObservableObject
{
    [ObservableProperty]
    private Guid id;

    [ObservableProperty]
    private string documentTypeText = "Паспорт";

    [ObservableProperty]
    private string series = string.Empty;

    [ObservableProperty]
    private string number = string.Empty;

    [ObservableProperty]
    private string issuedBy = string.Empty;

    [ObservableProperty]
    private DateTimeOffset? issueDate;

    [ObservableProperty]
    private string departmentCode = string.Empty;

    [ObservableProperty]
    private string organization = string.Empty;
}
