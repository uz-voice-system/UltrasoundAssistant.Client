using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;
using UltrasoundAssistant.DoctorClient.Helpers;
using UltrasoundAssistant.DoctorClient.Models.Commands.Patients;
using UltrasoundAssistant.DoctorClient.Models.Entity.Patients;
using UltrasoundAssistant.DoctorClient.Models.Enums;
using UltrasoundAssistant.DoctorClient.Models.Reads.Patients.Details;
using UltrasoundAssistant.DoctorClient.Models.Reads.Patients.Search;
using UltrasoundAssistant.DoctorClient.Services;
using SharedBoolFilterOption = UltrasoundAssistant.DoctorClient.ViewModels.BoolFilterOption;

namespace UltrasoundAssistant.DoctorClient.ViewModels.Patient;

public partial class PatientsViewModel : CrudPageViewModelBase<PatientAdminListItem>
{
    private readonly PatientApiService _patientService;

    private Guid? _editingPatientId;
    private int _editingPatientVersion;

    public ObservableCollection<PatientAdminListItem> Patients => Items;

    public ObservableCollection<string> Genders { get; } = new();

    public ObservableCollection<string> DocumentTypeOptions { get; } = new();

    public ObservableCollection<SharedBoolFilterOption> DeletedOptions { get; } = new();

    public ObservableCollection<EditablePatientDocumentItem> EditableDocuments { get; } = new();

    private bool _areAdditionalFiltersVisible;
    public bool AreAdditionalFiltersVisible
    {
        get => _areAdditionalFiltersVisible;
        set => SetProperty(ref _areAdditionalFiltersVisible, value);
    }

    private string _searchFullName = string.Empty;
    public string SearchFullName
    {
        get => _searchFullName;
        set => SetProperty(ref _searchFullName, value);
    }

    private DateTimeOffset? _filterBirthDate;
    public DateTimeOffset? FilterBirthDate
    {
        get => _filterBirthDate;
        set => SetProperty(ref _filterBirthDate, value);
    }

    private string _filterPhoneNumber = string.Empty;
    public string FilterPhoneNumber
    {
        get => _filterPhoneNumber;
        set => SetProperty(ref _filterPhoneNumber, value);
    }

    private string _selectedDocumentTypeText = "Любой тип документа";
    public string SelectedDocumentTypeText
    {
        get => _selectedDocumentTypeText;
        set => SetProperty(ref _selectedDocumentTypeText, value);
    }

    private string _filterDocumentSeries = string.Empty;
    public string FilterDocumentSeries
    {
        get => _filterDocumentSeries;
        set => SetProperty(ref _filterDocumentSeries, value);
    }

    private string _filterDocumentNumber = string.Empty;
    public string FilterDocumentNumber
    {
        get => _filterDocumentNumber;
        set => SetProperty(ref _filterDocumentNumber, value);
    }

    private SharedBoolFilterOption? _selectedDeletedOption;
    public SharedBoolFilterOption? SelectedDeletedOption
    {
        get => _selectedDeletedOption;
        set => SetProperty(ref _selectedDeletedOption, value);
    }

    private string _editFullName = string.Empty;
    public string EditFullName
    {
        get => _editFullName;
        set => SetProperty(ref _editFullName, value);
    }

    private DateTimeOffset? _editBirthDate;
    public DateTimeOffset? EditBirthDate
    {
        get => _editBirthDate;
        set => SetProperty(ref _editBirthDate, value);
    }

    private string? _editSelectedGender;
    public string? EditSelectedGender
    {
        get => _editSelectedGender;
        set => SetProperty(ref _editSelectedGender, value);
    }

    private string _editPhoneNumber = string.Empty;
    public string EditPhoneNumber
    {
        get => _editPhoneNumber;
        set => SetProperty(ref _editPhoneNumber, value);
    }

    private string _editEmail = string.Empty;
    public string EditEmail
    {
        get => _editEmail;
        set => SetProperty(ref _editEmail, value);
    }

    public ICommand LoadPatientsCommand { get; }

    public ICommand SearchPatientsCommand { get; }

    public ICommand ClearFiltersCommand { get; }

    public ICommand AddPatientCommand { get; }

    public ICommand EditPatientCommand { get; }

    public ICommand DeletePatientCommand { get; }

    public ICommand SavePatientCommand { get; }

    public ICommand CancelEditCommand { get; }

    public ICommand AddDocumentCommand { get; }

    public ICommand RemoveDocumentCommand { get; }

    public PatientsViewModel(
        MainWindowViewModel main,
        PatientApiService patientService)
        : base(main)
    {
        _patientService = patientService;

        Genders.Add("Мужской");
        Genders.Add("Женский");

        DocumentTypeOptions.Add("Любой тип документа");

        foreach (var type in Enum.GetValues<PatientDocumentType>())
            DocumentTypeOptions.Add(GetDocumentTypeText(type));

        SelectedDocumentTypeText = DocumentTypeOptions[0];

        DeletedOptions.Add(new SharedBoolFilterOption("Только активные", false));
        DeletedOptions.Add(new SharedBoolFilterOption("Активные и удалённые", true));
        SelectedDeletedOption = DeletedOptions[0];

        LoadPatientsCommand = new AsyncRelayCommand(LoadPatientsAsync);
        SearchPatientsCommand = new AsyncRelayCommand(SearchPatientsAsync);
        ClearFiltersCommand = new AsyncRelayCommand(ClearFiltersAsync);

        AddPatientCommand = new RelayCommandSync(_ => OpenEditPanelForAdd());

        EditPatientCommand = new RelayCommand<PatientAdminListItem?>(async patient =>
        {
            await OpenEditPanelForEditAsync(patient);
        });

        DeletePatientCommand = new RelayCommand<PatientAdminListItem?>(async patient =>
        {
            await DeletePatientAsync(patient);
        });

        SavePatientCommand = new AsyncRelayCommand(SavePatientAsync);
        CancelEditCommand = new RelayCommandSync(_ => CloseEditPanel());

        AddDocumentCommand = new RelayCommandSync(_ => AddDocument());

        RemoveDocumentCommand = new RelayCommand<EditablePatientDocumentItem?>(document =>
        {
            RemoveDocument(document);
        });
    }

    private async Task LoadPatientsAsync()
    {
        ClearError();
        IsEditPanelVisible = false;

        var result = await _patientService.GetAllAsync();

        if (result.IsSuccess && result.Data != null)
            ReplacePatients(result.Data);
        else
            SetError(result.ErrorMessage);
    }

    private async Task SearchPatientsAsync()
    {
        ClearError();

        var filter = new PatientSearchRequest
        {
            SearchText = null,

            FullName = string.IsNullOrWhiteSpace(SearchFullName)
                ? null
                : SearchFullName.Trim(),

            BirthDate = AreAdditionalFiltersVisible
                ? FilterBirthDate?.Date
                : null,

            PhoneNumber = AreAdditionalFiltersVisible && !string.IsNullOrWhiteSpace(FilterPhoneNumber)
                ? FilterPhoneNumber.Trim()
                : null,

            DocumentType = AreAdditionalFiltersVisible
                ? MapNullableDocumentType(SelectedDocumentTypeText)
                : null,

            DocumentSeries = AreAdditionalFiltersVisible && !string.IsNullOrWhiteSpace(FilterDocumentSeries)
                ? FilterDocumentSeries.Trim()
                : null,

            DocumentNumber = AreAdditionalFiltersVisible && !string.IsNullOrWhiteSpace(FilterDocumentNumber)
                ? FilterDocumentNumber.Trim()
                : null,

            IncludeDeleted = SelectedDeletedOption?.Value == true
        };

        var result = await _patientService.SearchAsync(filter);

        if (result.IsSuccess && result.Data != null)
            ReplacePatients(result.Data);
        else
            SetError(result.ErrorMessage);
    }

    private async Task ClearFiltersAsync()
    {
        SearchFullName = string.Empty;
        FilterBirthDate = null;
        FilterPhoneNumber = string.Empty;
        SelectedDocumentTypeText = DocumentTypeOptions[0];
        FilterDocumentSeries = string.Empty;
        FilterDocumentNumber = string.Empty;
        SelectedDeletedOption = DeletedOptions[0];

        await LoadPatientsAsync();
    }

    private void ReplacePatients(List<PatientSummaryDto> patients)
    {
        ReplaceItems(patients
            .Select(x => new PatientAdminListItem(x))
            .OrderBy(x => x.IsDeleted)
            .ThenBy(x => x.FullName)
            .ToList());
    }

    private void OpenEditPanelForAdd()
    {
        _editingPatientId = null;
        _editingPatientVersion = 0;

        OpenEditPanel("Добавить пациента");

        EditFullName = string.Empty;
        EditBirthDate = null;
        EditSelectedGender = Genders.Count > 0 ? Genders[0] : null;
        EditPhoneNumber = string.Empty;
        EditEmail = string.Empty;

        EditableDocuments.Clear();
    }

    private async Task OpenEditPanelForEditAsync(PatientAdminListItem? patient)
    {
        ClearError();

        if (patient == null)
            return;

        var result = await _patientService.GetByIdAsync(patient.Id);

        if (!result.IsSuccess || result.Data == null)
        {
            SetError(result.ErrorMessage ?? "Не удалось загрузить пациента.");
            return;
        }

        var full = result.Data;

        _editingPatientId = full.Id;
        _editingPatientVersion = full.Version;

        OpenEditPanel("Редактировать пациента");

        EditFullName = full.FullName;
        EditBirthDate = new DateTimeOffset(full.BirthDate);
        EditSelectedGender = full.Gender;
        EditPhoneNumber = full.PhoneNumber ?? string.Empty;
        EditEmail = full.Email ?? string.Empty;

        EditableDocuments.Clear();

        foreach (var document in full.Documents ?? [])
        {
            EditableDocuments.Add(new EditablePatientDocumentItem
            {
                Id = document.Id,
                DocumentTypeText = GetDocumentTypeText(document.DocumentType),
                Series = document.Series ?? string.Empty,
                Number = document.Number,
                IssuedBy = document.IssuedBy ?? string.Empty,
                IssueDate = document.IssueDate.HasValue
                    ? new DateTimeOffset(document.IssueDate.Value)
                    : null,
                DepartmentCode = document.DepartmentCode ?? string.Empty,
                Organization = document.Organization ?? string.Empty
            });
        }
    }

    protected override void CloseEditPanel()
    {
        base.CloseEditPanel();

        _editingPatientId = null;
        _editingPatientVersion = 0;

        EditFullName = string.Empty;
        EditBirthDate = null;
        EditSelectedGender = null;
        EditPhoneNumber = string.Empty;
        EditEmail = string.Empty;

        EditableDocuments.Clear();
    }

    private void AddDocument()
    {
        EditableDocuments.Add(new EditablePatientDocumentItem
        {
            Id = Guid.NewGuid(),
            DocumentTypeText = "Паспорт",
            Series = string.Empty,
            Number = string.Empty,
            IssuedBy = string.Empty,
            IssueDate = null,
            DepartmentCode = string.Empty,
            Organization = string.Empty
        });
    }

    private void RemoveDocument(EditablePatientDocumentItem? document)
    {
        if (document == null)
            return;

        EditableDocuments.Remove(document);
    }

    private async Task SavePatientAsync()
    {
        ClearError();

        if (string.IsNullOrWhiteSpace(EditFullName))
        {
            SetError("ФИО пациента обязательно.");
            return;
        }

        if (EditBirthDate == null)
        {
            SetError("Дата рождения обязательна.");
            return;
        }

        var documents = BuildDocuments();

        if (documents == null)
            return;

        if (_editingPatientId == null)
        {
            var command = new CreatePatientCommand
            {
                PatientId = Guid.NewGuid(),
                FullName = EditFullName.Trim(),
                BirthDate = ToUtcDate(EditBirthDate.Value),
                Gender = EditSelectedGender,
                PhoneNumber = string.IsNullOrWhiteSpace(EditPhoneNumber) ? null : EditPhoneNumber.Trim(),
                Email = string.IsNullOrWhiteSpace(EditEmail) ? null : EditEmail.Trim(),
                Documents = documents
            };

            var result = await _patientService.CreateAsync(command);

            if (!result.IsSuccess)
            {
                SetError(result.ErrorMessage);
                return;
            }
        }
        else
        {
            var command = new UpdatePatientCommand
            {
                PatientId = _editingPatientId.Value,
                ExpectedVersion = _editingPatientVersion,
                FullName = EditFullName.Trim(),
                BirthDate = ToUtcDate(EditBirthDate.Value),
                Gender = EditSelectedGender,
                PhoneNumber = string.IsNullOrWhiteSpace(EditPhoneNumber) ? null : EditPhoneNumber.Trim(),
                Email = string.IsNullOrWhiteSpace(EditEmail) ? null : EditEmail.Trim(),
                Documents = documents
            };

            var result = await _patientService.UpdateAsync(command);

            if (!result.IsSuccess)
            {
                SetError(result.ErrorMessage);
                return;
            }
        }

        CloseEditPanel();
        RefreshLaterIfCurrent(SearchPatientsAsync);
    }

    private List<PatientDocumentDto>? BuildDocuments()
    {
        var documents = new List<PatientDocumentDto>();

        foreach (var document in EditableDocuments)
        {
            if (string.IsNullOrWhiteSpace(document.Number))
            {
                SetError("У каждого документа должен быть указан номер.");
                return null;
            }

            documents.Add(new PatientDocumentDto
            {
                Id = document.Id == Guid.Empty ? Guid.NewGuid() : document.Id,
                DocumentType = MapDocumentType(document.DocumentTypeText),
                Series = string.IsNullOrWhiteSpace(document.Series) ? null : document.Series.Trim(),
                Number = document.Number.Trim(),
                IssuedBy = string.IsNullOrWhiteSpace(document.IssuedBy) ? null : document.IssuedBy.Trim(),
                IssueDate = document.IssueDate.HasValue
                    ? ToUtcDate(document.IssueDate.Value)
                    : null,
                DepartmentCode = string.IsNullOrWhiteSpace(document.DepartmentCode) ? null : document.DepartmentCode.Trim(),
                Organization = string.IsNullOrWhiteSpace(document.Organization) ? null : document.Organization.Trim()
            });
        }

        return documents;
    }

    private async Task DeletePatientAsync(PatientAdminListItem? patient)
    {
        ClearError();

        if (patient == null)
            return;

        var command = new DeletePatientCommand
        {
            PatientId = patient.Id,
            ExpectedVersion = patient.Version
        };

        var result = await _patientService.DeleteAsync(command);

        if (result.IsSuccess)
            RefreshLaterIfCurrent(SearchPatientsAsync);
        else
            SetError(result.ErrorMessage);
    }

    private static PatientDocumentType? MapNullableDocumentType(string? text)
    {
        return text switch
        {
            "Паспорт" => PatientDocumentType.Passport,
            "СНИЛС" => PatientDocumentType.Snils,
            "Полис ОМС" => PatientDocumentType.OmsPolicy,
            "Медицинская карта" => PatientDocumentType.MedicalCard,
            "Иной документ" => PatientDocumentType.Other,
            _ => null
        };
    }

    private static PatientDocumentType MapDocumentType(string? text)
    {
        return MapNullableDocumentType(text) ?? PatientDocumentType.Passport;
    }

    private static string GetDocumentTypeText(PatientDocumentType type)
    {
        return type switch
        {
            PatientDocumentType.Passport => "Паспорт",
            PatientDocumentType.Snils => "СНИЛС",
            PatientDocumentType.OmsPolicy => "Полис ОМС",
            PatientDocumentType.MedicalCard => "Медицинская карта",
            PatientDocumentType.Other => "Иной документ",
            _ => type.ToString()
        };
    }

    private static DateTime ToUtcDate(DateTimeOffset value)
    {
        var date = value.Date;
        return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
    }
}
