using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;
using UltrasoundAssistant.DoctorClient.Models.Commands.Patient;
using UltrasoundAssistant.DoctorClient.Models.Read.Patient;
using UltrasoundAssistant.DoctorClient.Services;
using UltrasoundAssistant.DoctorClient.Helpers;

namespace UltrasoundAssistant.DoctorClient.ViewModels.Patient;

public partial class PatientsViewModel : CrudPageViewModelBase<PatientDto>
{
    private readonly PatientApiService _patientService;

    public ObservableCollection<string> Genders { get; } = new();

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
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

    private Guid? _editingPatientId;
    private int _editingPatientVersion;

    public ObservableCollection<PatientDto> Patients => Items;

    public ICommand LoadPatientsCommand { get; }
    public ICommand SearchPatientsCommand { get; }
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

        LoadPatientsCommand = new AsyncRelayCommand(LoadPatientsAsync);
        SearchPatientsCommand = new AsyncRelayCommand(SearchPatientsAsync);
        AddPatientCommand = new RelayCommandSync(_ => OpenEditPanelForAdd());
        EditPatientCommand = new RelayCommand<PatientDto?>(OpenEditPanelForEdit);
        DeletePatientCommand = new RelayCommand<PatientDto?>(async p => await DeletePatientAsync(p));
        SavePatientCommand = new AsyncRelayCommand(SavePatientAsync);
        CancelEditCommand = new RelayCommandSync(_ => CloseEditPanel());
    }

    private async Task LoadPatientsAsync()
    {
        ClearError();
        Patients.Clear();
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
        Patients.Clear();

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await LoadPatientsAsync();
            return;
        }

        var result = await _patientService.SearchByFullNameAsync(SearchText);

        if (result.IsSuccess && result.Data != null)
            ReplaceItems(result.Data);
        else
            SetError(result.ErrorMessage);
    }

    private void OpenEditPanelForAdd()
    {
        _editingPatientId = null;
        _editingPatientVersion = 0;

        OpenEditPanel("Добавить пациента");
        EditFullName = string.Empty;
        EditBirthDate = null;
        EditSelectedGender = Genders.Count > 0 ? Genders[0] : null;
    }

    private void OpenEditPanelForEdit(PatientDto? patient)
    {
        if (patient == null)
            return;

        _editingPatientId = patient.Id;
        _editingPatientVersion = patient.Version;

        OpenEditPanel("Редактировать пациента");
        EditFullName = patient.FullName;
        EditBirthDate = new DateTimeOffset(patient.BirthDate);
        EditSelectedGender = patient.Gender;
    }

    protected override void CloseEditPanel()
    {
        base.CloseEditPanel();
        _editingPatientId = null;
        _editingPatientVersion = 0;
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
                CommandId = Guid.NewGuid(),
                Id = Guid.NewGuid(),
                FullName = EditFullName.Trim(),
                BirthDate = EditBirthDate.Value.DateTime,
                Gender = EditSelectedGender
            };

            var result = await _patientService.CreateAsync(command);

            if (result.IsSuccess)
            {
                CloseEditPanel();
                RefreshLaterIfCurrent(LoadPatientsAsync);
            }
            else
            {
                SetError(result.ErrorMessage);
            }
        }
        else
        {
            var command = new UpdatePatientCommand
            {
                CommandId = Guid.NewGuid(),
                PatientId = _editingPatientId.Value,
                ExpectedVersion = _editingPatientVersion,
                FullName = EditFullName.Trim(),
                BirthDate = EditBirthDate.Value.DateTime,
                Gender = EditSelectedGender
            };

            var result = await _patientService.UpdateAsync(command);

            if (result.IsSuccess)
            {
                CloseEditPanel();
                RefreshLaterIfCurrent(LoadPatientsAsync);
            }
            else
            {
                SetError(result.ErrorMessage);
            }
        }
    }

    private async Task DeletePatientAsync(PatientDto? patient)
    {
        ClearError();

        if (patient == null)
            return;

        var command = new DeactivatePatientCommand
        {
            CommandId = Guid.NewGuid(),
            PatientId = patient.Id,
            ExpectedVersion = patient.Version,
            Reason = "Деактивирован пользователем"
        };

        var result = await _patientService.DeactivateAsync(command);

        if (result.IsSuccess)
            RefreshLaterIfCurrent(LoadPatientsAsync);
        else
            SetError(result.ErrorMessage);
    }
}