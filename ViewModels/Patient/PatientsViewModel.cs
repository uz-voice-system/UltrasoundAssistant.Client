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

namespace UltrasoundAssistant.DoctorClient.ViewModels.Patient;

public partial class PatientsViewModel : CrudPageViewModelBase<PatientSummaryDto>
{
    private readonly PatientApiService _patientService;

    private Guid? _editingPatientId;
    private int _editingPatientVersion;
    private List<PatientDocumentDto> _editDocuments = [];

    public ObservableCollection<PatientSummaryDto> Patients => Items;

    public ObservableCollection<string> Genders { get; } = new();
    public ObservableCollection<PatientDocumentType?> DocumentTypes { get; } = new();

    private bool _areAdditionalFiltersVisible;
    public bool AreAdditionalFiltersVisible
    {
        get => _areAdditionalFiltersVisible;
        set => SetProperty(ref _areAdditionalFiltersVisible, value);
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private string _filterFullName = string.Empty;
    public string FilterFullName
    {
        get => _filterFullName;
        set => SetProperty(ref _filterFullName, value);
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

    private PatientDocumentType? _filterDocumentType;
    public PatientDocumentType? FilterDocumentType
    {
        get => _filterDocumentType;
        set => SetProperty(ref _filterDocumentType, value);
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

    private bool _includeDeleted;
    public bool IncludeDeleted
    {
        get => _includeDeleted;
        set => SetProperty(ref _includeDeleted, value);
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
    public ICommand ToggleAdditionalFiltersCommand { get; }

    public ICommand AddPatientCommand { get; }
    public ICommand EditPatientCommand { get; }
    public ICommand DeletePatientCommand { get; }
    public ICommand SavePatientCommand { get; }
    public ICommand CancelEditCommand { get; }

    public PatientsViewModel(MainWindowViewModel main, PatientApiService patientService)
        : base(main)
    {
        _patientService = patientService;

        Genders.Add("Мужской");
        Genders.Add("Женский");

        DocumentTypes.Add(null);
        foreach (var type in Enum.GetValues<PatientDocumentType>())
            DocumentTypes.Add(type);

        LoadPatientsCommand = new AsyncRelayCommand(LoadPatientsAsync);
        SearchPatientsCommand = new AsyncRelayCommand(SearchPatientsAsync);
        ClearFiltersCommand = new AsyncRelayCommand(ClearFiltersAsync);

        ToggleAdditionalFiltersCommand = new RelayCommandSync(_ => { AreAdditionalFiltersVisible = !AreAdditionalFiltersVisible; });

        AddPatientCommand = new RelayCommandSync(_ => OpenEditPanelForAdd());

        EditPatientCommand = new RelayCommand<PatientSummaryDto?>(async p => { await OpenEditPanelForEditAsync(p); });

        DeletePatientCommand = new RelayCommand<PatientSummaryDto?>(async p => { await DeletePatientAsync(p); });

        SavePatientCommand = new AsyncRelayCommand(SavePatientAsync);

        CancelEditCommand = new RelayCommandSync(_ => CloseEditPanel());
    }

    private async Task LoadPatientsAsync()
    {
        ClearError();
        IsEditPanelVisible = false;

        var result = await _patientService.GetAllAsync();

        if (result.IsSuccess && result.Data != null)
            ReplaceItems(result.Data);
        else
            SetError(result.ErrorMessage);
    }

    private async Task SearchPatientsAsync()
    {
        ClearError();

        var filter = new PatientSearchRequest
        {
            SearchText = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim(),
            FullName = string.IsNullOrWhiteSpace(FilterFullName) ? null : FilterFullName.Trim(),
            BirthDate = FilterBirthDate?.DateTime.Date,
            PhoneNumber = string.IsNullOrWhiteSpace(FilterPhoneNumber) ? null : FilterPhoneNumber.Trim(),
            DocumentType = FilterDocumentType,
            DocumentSeries = string.IsNullOrWhiteSpace(FilterDocumentSeries) ? null : FilterDocumentSeries.Trim(),
            DocumentNumber = string.IsNullOrWhiteSpace(FilterDocumentNumber) ? null : FilterDocumentNumber.Trim(),
            IncludeDeleted = IncludeDeleted
        };

        var result = await _patientService.SearchAsync(filter);

        if (result.IsSuccess && result.Data != null)
            ReplaceItems(result.Data);
        else
            SetError(result.ErrorMessage);
    }

    private async Task ClearFiltersAsync()
    {
        SearchText = string.Empty;
        FilterFullName = string.Empty;
        FilterBirthDate = null;
        FilterPhoneNumber = string.Empty;
        FilterDocumentType = null;
        FilterDocumentSeries = string.Empty;
        FilterDocumentNumber = string.Empty;
        IncludeDeleted = false;

        await LoadPatientsAsync();
    }

    private void OpenEditPanelForAdd()
    {
        _editingPatientId = null;
        _editingPatientVersion = 0;
        _editDocuments = [];

        OpenEditPanel("Добавить пациента");

        EditFullName = string.Empty;
        EditBirthDate = null;
        EditSelectedGender = Genders.Count > 0 ? Genders[0] : null;
        EditPhoneNumber = string.Empty;
        EditEmail = string.Empty;
    }

    private async Task OpenEditPanelForEditAsync(PatientSummaryDto? patient)
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
        _editDocuments = full.Documents ?? [];

        OpenEditPanel("Редактировать пациента");

        EditFullName = full.FullName;
        EditBirthDate = new DateTimeOffset(full.BirthDate);
        EditSelectedGender = full.Gender;
        EditPhoneNumber = full.PhoneNumber ?? string.Empty;
        EditEmail = full.Email ?? string.Empty;
    }

    protected override void CloseEditPanel()
    {
        base.CloseEditPanel();

        _editingPatientId = null;
        _editingPatientVersion = 0;
        _editDocuments = [];
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

        if (_editingPatientId == null)
        {
            var command = new CreatePatientCommand
            {
                PatientId = Guid.NewGuid(),
                FullName = EditFullName.Trim(),
                BirthDate = EditBirthDate.Value.DateTime.Date,
                Gender = EditSelectedGender,
                PhoneNumber = string.IsNullOrWhiteSpace(EditPhoneNumber) ? null : EditPhoneNumber.Trim(),
                Email = string.IsNullOrWhiteSpace(EditEmail) ? null : EditEmail.Trim(),
                Documents = _editDocuments
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
                BirthDate = EditBirthDate.Value.DateTime.Date,
                Gender = EditSelectedGender,
                PhoneNumber = string.IsNullOrWhiteSpace(EditPhoneNumber) ? null : EditPhoneNumber.Trim(),
                Email = string.IsNullOrWhiteSpace(EditEmail) ? null : EditEmail.Trim(),
                Documents = _editDocuments
            };

            var result = await _patientService.UpdateAsync(command);

            if (!result.IsSuccess)
            {
                SetError(result.ErrorMessage);
                return;
            }
        }

        CloseEditPanel();
        await SearchPatientsAsync();
    }

    private async Task DeletePatientAsync(PatientSummaryDto? patient)
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
            await SearchPatientsAsync();
        else
            SetError(result.ErrorMessage);
    }
}
