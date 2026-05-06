using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;
using UltrasoundAssistant.DoctorClient.Helpers;
using UltrasoundAssistant.DoctorClient.Models.Reads.Templates.Details;
using UltrasoundAssistant.DoctorClient.Models.Reads.Templates.Search;
using UltrasoundAssistant.DoctorClient.Services;

namespace UltrasoundAssistant.DoctorClient.ViewModels.Template;

public partial class TemplatesViewModel : CrudPageViewModelBase<TemplateSummaryDto>
{
    private readonly TemplateApiService _templateService;

    public ObservableCollection<EditableTemplateKeyword> EditableKeywords { get; } = new();

    private string _editTemplateName = string.Empty;
    public string EditTemplateName
    {
        get => _editTemplateName;
        set => SetProperty(ref _editTemplateName, value);
    }

    private string _newKeywordPhrase = string.Empty;
    public string NewKeywordPhrase
    {
        get => _newKeywordPhrase;
        set => SetProperty(ref _newKeywordPhrase, value);
    }

    private string _newKeywordFieldName = string.Empty;
    public string NewKeywordFieldName
    {
        get => _newKeywordFieldName;
        set => SetProperty(ref _newKeywordFieldName, value);
    }

    private Guid? _editingTemplateId;
    private int _editingTemplateVersion;

    public ObservableCollection<TemplateSummaryDto> Templates => Items;

    public ICommand LoadTemplatesCommand { get; }
    public ICommand AddTemplateCommand { get; }
    public ICommand EditTemplateCommand { get; }
    public ICommand DeleteTemplateCommand { get; }
    public ICommand SaveTemplateCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand AddKeywordCommand { get; }
    public ICommand RemoveKeywordCommand { get; }

    public TemplatesViewModel(MainWindowViewModel main, TemplateApiService templateService)
        : base(main)
    {
        _templateService = templateService;

        LoadTemplatesCommand = new AsyncRelayCommand(LoadTemplatesAsync);
        AddTemplateCommand = new RelayCommandSync(_ => OpenEditPanelForAdd());
        EditTemplateCommand = new RelayCommand<TemplateDto?>(OpenEditPanelForEdit);
        DeleteTemplateCommand = new RelayCommand<TemplateDto?>(async t => await DeleteTemplateAsync(t));
        SaveTemplateCommand = new AsyncRelayCommand(SaveTemplateAsync);
        CancelEditCommand = new RelayCommandSync(_ => CloseEditPanel());
        AddKeywordCommand = new RelayCommandSync(_ => AddKeyword());
        RemoveKeywordCommand = new RelayCommand<EditableTemplateKeyword?>(RemoveKeyword);
    }

    private async Task LoadTemplatesAsync()
    {
        ClearError();
        Templates.Clear();
        IsEditPanelVisible = false;

        var result = await _templateService.GetAllForAdminAsync();

        if (result.IsSuccess && result.Data != null)
            ReplaceItems(result.Data);
        else
            SetError(result.ErrorMessage);
    }

    private void OpenEditPanelForAdd()
    {
        _editingTemplateId = null;
        _editingTemplateVersion = 0;

        OpenEditPanel("Добавить шаблон");
        EditTemplateName = string.Empty;

        EditableKeywords.Clear();
        NewKeywordPhrase = string.Empty;
        NewKeywordFieldName = string.Empty;
    }

    private void OpenEditPanelForEdit(TemplateDto? template)
    {
        if (template == null)
            return;

        _editingTemplateId = template.Id;
        _editingTemplateVersion = template.Version;

        OpenEditPanel("Редактировать шаблон");
        EditTemplateName = template.Name;

        EditableKeywords.Clear();
        //foreach (var keyword in template.Keywords)
        //{
        //    EditableKeywords.Add(new EditableTemplateKeyword
        //    {
        //        Phrase = keyword.Phrase,
        //        FieldName = keyword.TargetField
        //    });
        //}

        NewKeywordPhrase = string.Empty;
        NewKeywordFieldName = string.Empty;
    }

    protected override void CloseEditPanel()
    {
        base.CloseEditPanel();

        _editingTemplateId = null;
        _editingTemplateVersion = 0;
        EditableKeywords.Clear();
        NewKeywordPhrase = string.Empty;
        NewKeywordFieldName = string.Empty;
    }

    private void AddKeyword()
    {
        ClearError();

        if (string.IsNullOrWhiteSpace(NewKeywordPhrase) || string.IsNullOrWhiteSpace(NewKeywordFieldName))
        {
            SetError("Фраза и имя поля обязательны.");
            return;
        }

        var alreadyExists = EditableKeywords.Any(k =>
            string.Equals(k.Phrase.Trim(), NewKeywordPhrase.Trim(), StringComparison.OrdinalIgnoreCase));

        if (alreadyExists)
        {
            SetError("Такое ключевое слово уже добавлено.");
            return;
        }

        EditableKeywords.Add(new EditableTemplateKeyword
        {
            Phrase = NewKeywordPhrase.Trim(),
            FieldName = NewKeywordFieldName.Trim()
        });

        NewKeywordPhrase = string.Empty;
        NewKeywordFieldName = string.Empty;
    }

    private void RemoveKeyword(EditableTemplateKeyword? keyword)
    {
        if (keyword == null)
            return;

        EditableKeywords.Remove(keyword);
    }

    private async Task SaveTemplateAsync()
    {
        ClearError();

        if (string.IsNullOrWhiteSpace(EditTemplateName))
        {
            SetError("Название шаблона обязательно.");
            return;
        }

        var keywords = EditableKeywords
            .Where(k => !string.IsNullOrWhiteSpace(k.Phrase) && !string.IsNullOrWhiteSpace(k.FieldName))
            .GroupBy(k => k.Phrase.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.First().FieldName.Trim(),
                StringComparer.OrdinalIgnoreCase);

        //if (_editingTemplateId == null)
        //{
        //    var command = new CreateTemplateCommand
        //    {
        //        CommandId = Guid.NewGuid(),
        //        TemplateId = Guid.NewGuid(),
        //        Name = EditTemplateName.Trim(),
        //        Keywords = keywords
        //    };

        //    var result = await _templateService.CreateAsync(command);

        //    if (result.IsSuccess)
        //    {
        //        CloseEditPanel();
        //        RefreshLaterIfCurrent(LoadTemplatesAsync);
        //    }
        //    else
        //    {
        //        SetError(result.ErrorMessage);
        //    }
        //}
        //else
        //{
        //    var command = new UpdateTemplateCommand
        //    {
        //        CommandId = Guid.NewGuid(),
        //        TemplateId = _editingTemplateId.Value,
        //        ExpectedVersion = _editingTemplateVersion,
        //        Name = EditTemplateName.Trim(),
        //        Keywords = keywords
        //    };

        //    var result = await _templateService.UpdateAsync(command);

        //    if (result.IsSuccess)
        //    {
        //        CloseEditPanel();
        //        RefreshLaterIfCurrent(LoadTemplatesAsync);
        //    }
        //    else
        //    {
        //        SetError(result.ErrorMessage);
        //    }
        //}
    }

    private async Task DeleteTemplateAsync(TemplateDto? template)
    {
        ClearError();

        if (template == null)
            return;

        //var command = new DeleteTemplateCommand
        //{
        //    CommandId = Guid.NewGuid(),
        //    TemplateId = template.Id,
        //    ExpectedVersion = template.Version
        //};

        //var result = await _templateService.DeleteAsync(command);

        //if (result.IsSuccess)
        //    RefreshLaterIfCurrent(LoadTemplatesAsync);
        //else
        //    SetError(result.ErrorMessage);
    }
}