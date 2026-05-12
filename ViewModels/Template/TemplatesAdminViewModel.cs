using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using UltrasoundAssistant.DoctorClient.Helpers;
using UltrasoundAssistant.DoctorClient.Models.Commands.Templates;
using UltrasoundAssistant.DoctorClient.Models.Entity.Templates;
using UltrasoundAssistant.DoctorClient.Models.Reads.Templates.Admin;
using UltrasoundAssistant.DoctorClient.Services;
using SharedBoolFilterOption = UltrasoundAssistant.DoctorClient.ViewModels.BoolFilterOption;

namespace UltrasoundAssistant.DoctorClient.ViewModels.Template;

public class TemplatesAdminViewModel : CrudPageViewModelBase<TemplateAdminListItem>
{
    private readonly TemplateApiService _templateService;

    private Guid? _editingTemplateId;
    private int _editingTemplateVersion;

    public ObservableCollection<TemplateAdminListItem> Templates => Items;
    public ObservableCollection<EditableTemplateBlockItem> EditableBlocks { get; } = new();

    public ObservableCollection<string> FieldTypeFilterOptions { get; } = new();
    public ObservableCollection<string> FieldRoleFilterOptions { get; } = new();

    public ObservableCollection<string> FieldTypeEditOptions { get; } = new();
    public ObservableCollection<string> FieldRoleEditOptions { get; } = new();

    public ObservableCollection<SharedBoolFilterOption> HasNormOptions { get; } = new();

    private bool _areAdditionalFiltersVisible;
    public bool AreAdditionalFiltersVisible
    {
        get => _areAdditionalFiltersVisible;
        set => SetProperty(ref _areAdditionalFiltersVisible, value);
    }

    private string _filterTemplateName = string.Empty;
    public string FilterTemplateName
    {
        get => _filterTemplateName;
        set => SetProperty(ref _filterTemplateName, value);
    }

    private string _filterBlockName = string.Empty;
    public string FilterBlockName
    {
        get => _filterBlockName;
        set => SetProperty(ref _filterBlockName, value);
    }

    private string _filterFieldName = string.Empty;
    public string FilterFieldName
    {
        get => _filterFieldName;
        set => SetProperty(ref _filterFieldName, value);
    }

    private string _filterFieldDisplayName = string.Empty;
    public string FilterFieldDisplayName
    {
        get => _filterFieldDisplayName;
        set => SetProperty(ref _filterFieldDisplayName, value);
    }

    private string _filterPhrase = string.Empty;
    public string FilterPhrase
    {
        get => _filterPhrase;
        set => SetProperty(ref _filterPhrase, value);
    }

    private string _filterFieldTypeText = "Все типы полей";
    public string FilterFieldTypeText
    {
        get => _filterFieldTypeText;
        set => SetProperty(ref _filterFieldTypeText, value);
    }

    private string _filterFieldRoleText = "Все роли поля";
    public string FilterFieldRoleText
    {
        get => _filterFieldRoleText;
        set => SetProperty(ref _filterFieldRoleText, value);
    }

    private SharedBoolFilterOption? _selectedHasNormOption;
    public SharedBoolFilterOption? SelectedHasNormOption
    {
        get => _selectedHasNormOption;
        set => SetProperty(ref _selectedHasNormOption, value);
    }

    private bool _includeDeleted;
    public bool IncludeDeleted
    {
        get => _includeDeleted;
        set => SetProperty(ref _includeDeleted, value);
    }

    private string _editTemplateName = string.Empty;
    public string EditTemplateName
    {
        get => _editTemplateName;
        set => SetProperty(ref _editTemplateName, value);
    }

    private int _editDefaultAppointmentDurationMinutes = 30;
    public int EditDefaultAppointmentDurationMinutes
    {
        get => _editDefaultAppointmentDurationMinutes;
        set => SetProperty(ref _editDefaultAppointmentDurationMinutes, value);
    }

    private string _newBlockName = string.Empty;
    public string NewBlockName
    {
        get => _newBlockName;
        set => SetProperty(ref _newBlockName, value);
    }

    private string _newBlockPhrases = string.Empty;
    public string NewBlockPhrases
    {
        get => _newBlockPhrases;
        set => SetProperty(ref _newBlockPhrases, value);
    }

    private string _newBlockDefaultFieldName = string.Empty;
    public string NewBlockDefaultFieldName
    {
        get => _newBlockDefaultFieldName;
        set => SetProperty(ref _newBlockDefaultFieldName, value);
    }

    public ICommand LoadTemplatesCommand { get; }
    public ICommand SearchTemplatesCommand { get; }
    public ICommand ClearFiltersCommand { get; }

    public ICommand AddTemplateCommand { get; }
    public ICommand EditTemplateCommand { get; }
    public ICommand DeleteTemplateCommand { get; }
    public ICommand SaveTemplateCommand { get; }
    public ICommand CancelEditCommand { get; }

    public ICommand AddBlockCommand { get; }
    public ICommand RemoveBlockCommand { get; }
    public ICommand AddFieldCommand { get; }
    public ICommand RemoveFieldCommand { get; }

    public TemplatesAdminViewModel(
        MainWindowViewModel main,
        TemplateApiService templateService)
        : base(main)
    {
        _templateService = templateService;

        FieldTypeFilterOptions.Add("Все типы полей");
        FieldTypeFilterOptions.Add("Текст");
        FieldTypeFilterOptions.Add("Число");
        FieldTypeFilterOptions.Add("Число с единицей измерения");
        FilterFieldTypeText = FieldTypeFilterOptions[0];

        FieldRoleFilterOptions.Add("Все роли поля");
        FieldRoleFilterOptions.Add("Обычное поле");
        FieldRoleFilterOptions.Add("Описание исследования");
        FieldRoleFilterOptions.Add("Заключение исследования");
        FilterFieldRoleText = FieldRoleFilterOptions[0];

        FieldTypeEditOptions.Add("Текст");
        FieldTypeEditOptions.Add("Число");
        FieldTypeEditOptions.Add("Число с единицей измерения");

        FieldRoleEditOptions.Add("Обычное поле");
        FieldRoleEditOptions.Add("Описание исследования");
        FieldRoleEditOptions.Add("Заключение исследования");

        HasNormOptions.Add(new SharedBoolFilterOption("Все поля: с нормой и без нормы", null));
        HasNormOptions.Add(new SharedBoolFilterOption("Только поля, где задана норма", true));
        HasNormOptions.Add(new SharedBoolFilterOption("Только поля без нормы", false));
        SelectedHasNormOption = HasNormOptions[0];

        LoadTemplatesCommand = new AsyncRelayCommand(LoadTemplatesAsync);
        SearchTemplatesCommand = new AsyncRelayCommand(SearchTemplatesAsync);
        ClearFiltersCommand = new AsyncRelayCommand(ClearFiltersAsync);

        AddTemplateCommand = new RelayCommandSync(_ => OpenEditPanelForAdd());

        EditTemplateCommand = new RelayCommand<TemplateAdminListItem?>(async item =>
        {
            await OpenEditPanelForEditAsync(item);
        });

        DeleteTemplateCommand = new RelayCommand<TemplateAdminListItem?>(async item =>
        {
            await DeleteTemplateAsync(item);
        });

        SaveTemplateCommand = new AsyncRelayCommand(SaveTemplateAsync);
        CancelEditCommand = new RelayCommandSync(_ => CloseEditPanel());

        AddBlockCommand = new RelayCommandSync(_ => AddBlock());

        RemoveBlockCommand = new RelayCommand<EditableTemplateBlockItem?>(block =>
        {
            RemoveBlock(block);
        });

        AddFieldCommand = new RelayCommand<EditableTemplateBlockItem?>(block =>
        {
            AddField(block);
        });

        RemoveFieldCommand = new RelayCommand<EditableTemplateFieldItem?>(field =>
        {
            RemoveField(field);
        });
    }

    private async Task LoadTemplatesAsync()
    {
        ClearError();
        IsEditPanelVisible = false;

        var result = await _templateService.GetAllForAdminAsync();

        if (!result.IsSuccess || result.Data == null)
        {
            SetError(result.ErrorMessage);
            return;
        }

        await ReplaceTemplatesAsync(result.Data);
    }

    private async Task SearchTemplatesAsync()
    {
        ClearError();

        var filter = new TemplateAdminSearchRequest
        {
            TemplateName = string.IsNullOrWhiteSpace(FilterTemplateName)
                ? null
                : FilterTemplateName.Trim(),

            BlockName = AreAdditionalFiltersVisible && !string.IsNullOrWhiteSpace(FilterBlockName)
                ? FilterBlockName.Trim()
                : null,

            FieldName = AreAdditionalFiltersVisible && !string.IsNullOrWhiteSpace(FilterFieldName)
                ? FilterFieldName.Trim()
                : null,

            FieldDisplayName = AreAdditionalFiltersVisible && !string.IsNullOrWhiteSpace(FilterFieldDisplayName)
                ? FilterFieldDisplayName.Trim()
                : null,

            Phrase = AreAdditionalFiltersVisible && !string.IsNullOrWhiteSpace(FilterPhrase)
                ? FilterPhrase.Trim()
                : null,

            FieldType = AreAdditionalFiltersVisible ? MapNullableFieldType(FilterFieldTypeText) : null,
            FieldRole = AreAdditionalFiltersVisible ? MapNullableFieldRole(FilterFieldRoleText) : null,
            HasNorm = AreAdditionalFiltersVisible ? SelectedHasNormOption?.Value : null,
            IncludeDeleted = IncludeDeleted
        };

        var result = await _templateService.SearchForAdminAsync(filter);

        if (!result.IsSuccess || result.Data == null)
        {
            SetError(result.ErrorMessage);
            return;
        }

        await ReplaceTemplatesAsync(result.Data);
    }

    private async Task ReplaceTemplatesAsync(List<TemplateAdminSearchResultDto> source)
    {
        var result = new List<TemplateAdminListItem>();

        foreach (var item in source)
        {
            if (item.Template.Id != Guid.Empty)
            {
                var detailResult = await _templateService.GetByIdAsync(item.Template.Id);

                if (detailResult.IsSuccess && detailResult.Data != null)
                    item.Template = detailResult.Data;
            }

            result.Add(new TemplateAdminListItem(item));
        }

        ReplaceItems(result
            .OrderBy(x => x.IsDeleted)
            .ThenBy(x => x.Name)
            .ToList());
    }

    private async Task ClearFiltersAsync()
    {
        FilterTemplateName = string.Empty;
        FilterBlockName = string.Empty;
        FilterFieldName = string.Empty;
        FilterFieldDisplayName = string.Empty;
        FilterPhrase = string.Empty;
        FilterFieldTypeText = FieldTypeFilterOptions[0];
        FilterFieldRoleText = FieldRoleFilterOptions[0];
        SelectedHasNormOption = HasNormOptions[0];
        IncludeDeleted = false;

        await LoadTemplatesAsync();
    }

    private void OpenEditPanelForAdd()
    {
        _editingTemplateId = null;
        _editingTemplateVersion = 0;

        OpenEditPanel("Добавить шаблон");

        EditTemplateName = string.Empty;
        EditDefaultAppointmentDurationMinutes = 30;
        EditableBlocks.Clear();

        NewBlockName = string.Empty;
        NewBlockPhrases = string.Empty;
        NewBlockDefaultFieldName = string.Empty;
    }

    private async Task OpenEditPanelForEditAsync(TemplateAdminListItem? item)
    {
        ClearError();

        if (item == null)
            return;

        var result = await _templateService.GetByIdAsync(item.Id);

        if (!result.IsSuccess || result.Data == null)
        {
            SetError(result.ErrorMessage ?? "Не удалось загрузить шаблон.");
            return;
        }

        var template = result.Data;

        _editingTemplateId = template.Id;
        _editingTemplateVersion = template.Version;

        OpenEditPanel("Редактировать шаблон");

        EditTemplateName = template.Name;
        EditDefaultAppointmentDurationMinutes = template.DefaultAppointmentDurationMinutes;

        EditableBlocks.Clear();

        foreach (var block in template.Blocks.OrderBy(x => x.Position))
        {
            var blockItem = new EditableTemplateBlockItem
            {
                Id = block.Id,
                Name = block.Name,
                Position = block.Position,
                PhrasesText = JoinPhrases(block.Phrases),
                DefaultFieldName = block.DefaultFieldName ?? string.Empty
            };

            foreach (var field in block.Fields.OrderBy(x => x.Position))
            {
                blockItem.Fields.Add(new EditableTemplateFieldItem
                {
                    Id = field.Id,
                    FieldName = field.FieldName,
                    DisplayName = field.DisplayName,
                    Position = field.Position,
                    PhrasesText = JoinPhrases(field.Phrases),
                    TypeText = GetFieldTypeText(field.Type),
                    RoleText = GetFieldRoleText(field.Role),
                    HasNorm = field.Norm != null,
                    NormMin = field.Norm?.Min?.ToString("0.##", CultureInfo.CurrentCulture) ?? string.Empty,
                    NormMax = field.Norm?.Max?.ToString("0.##", CultureInfo.CurrentCulture) ?? string.Empty,
                    NormUnit = field.Norm?.Unit ?? string.Empty,
                    NormNormalText = field.Norm?.NormalText ?? string.Empty
                });
            }

            EditableBlocks.Add(blockItem);
        }

        NewBlockName = string.Empty;
        NewBlockPhrases = string.Empty;
        NewBlockDefaultFieldName = string.Empty;
    }

    protected override void CloseEditPanel()
    {
        base.CloseEditPanel();

        _editingTemplateId = null;
        _editingTemplateVersion = 0;

        EditTemplateName = string.Empty;
        EditDefaultAppointmentDurationMinutes = 30;
        EditableBlocks.Clear();

        NewBlockName = string.Empty;
        NewBlockPhrases = string.Empty;
        NewBlockDefaultFieldName = string.Empty;
    }

    private void AddBlock()
    {
        ClearError();

        if (string.IsNullOrWhiteSpace(NewBlockName))
        {
            SetError("Название блока обязательно.");
            return;
        }

        EditableBlocks.Add(new EditableTemplateBlockItem
        {
            Id = Guid.NewGuid(),
            Name = NewBlockName.Trim(),
            Position = EditableBlocks.Count + 1,
            PhrasesText = NewBlockPhrases.Trim(),
            DefaultFieldName = NewBlockDefaultFieldName.Trim()
        });

        NewBlockName = string.Empty;
        NewBlockPhrases = string.Empty;
        NewBlockDefaultFieldName = string.Empty;
    }

    private void RemoveBlock(EditableTemplateBlockItem? block)
    {
        if (block == null)
            return;

        EditableBlocks.Remove(block);
        RecalculatePositions();
    }

    private void AddField(EditableTemplateBlockItem? block)
    {
        ClearError();

        if (block == null)
            return;

        block.Fields.Add(new EditableTemplateFieldItem
        {
            Id = Guid.NewGuid(),
            FieldName = $"field_{block.Fields.Count + 1}",
            DisplayName = "Новое поле",
            Position = block.Fields.Count + 1,
            TypeText = "Текст",
            RoleText = "Обычное поле",
            HasNorm = false
        });
    }

    private void RemoveField(EditableTemplateFieldItem? field)
    {
        if (field == null)
            return;

        foreach (var block in EditableBlocks)
        {
            if (block.Fields.Remove(field))
            {
                RecalculatePositions();
                return;
            }
        }
    }

    private async Task SaveTemplateAsync()
    {
        ClearError();

        if (string.IsNullOrWhiteSpace(EditTemplateName))
        {
            SetError("Название шаблона обязательно.");
            return;
        }

        if (EditDefaultAppointmentDurationMinutes <= 0)
        {
            SetError("Длительность приёма должна быть больше нуля.");
            return;
        }

        var blocks = BuildBlocks();

        if (blocks == null)
            return;

        if (_editingTemplateId == null)
        {
            var command = new CreateTemplateCommand
            {
                TemplateId = Guid.NewGuid(),
                Name = EditTemplateName.Trim(),
                DefaultAppointmentDurationMinutes = EditDefaultAppointmentDurationMinutes,
                Blocks = blocks
            };

            var result = await _templateService.CreateAsync(command);

            if (!result.IsSuccess)
            {
                SetError(result.ErrorMessage);
                return;
            }
        }
        else
        {
            var command = new UpdateTemplateCommand
            {
                TemplateId = _editingTemplateId.Value,
                ExpectedVersion = _editingTemplateVersion,
                Name = EditTemplateName.Trim(),
                DefaultAppointmentDurationMinutes = EditDefaultAppointmentDurationMinutes,
                Blocks = blocks
            };

            var result = await _templateService.UpdateAsync(command);

            if (!result.IsSuccess)
            {
                SetError(result.ErrorMessage);
                return;
            }
        }

        CloseEditPanel();
        RefreshLaterIfCurrent(SearchTemplatesAsync);
    }

    private List<TemplateBlockDto>? BuildBlocks()
    {
        RecalculatePositions();

        var result = new List<TemplateBlockDto>();

        foreach (var block in EditableBlocks)
        {
            if (string.IsNullOrWhiteSpace(block.Name))
            {
                SetError("У каждого блока должно быть название.");
                return null;
            }

            var blockDto = new TemplateBlockDto
            {
                Id = block.Id == Guid.Empty ? Guid.NewGuid() : block.Id,
                Name = block.Name.Trim(),
                Position = block.Position,
                Phrases = SplitPhrases(block.PhrasesText),
                DefaultFieldName = string.IsNullOrWhiteSpace(block.DefaultFieldName)
                    ? null
                    : block.DefaultFieldName.Trim(),
                Fields = []
            };

            foreach (var field in block.Fields)
            {
                if (string.IsNullOrWhiteSpace(field.FieldName))
                {
                    SetError($"В блоке «{block.Name}» есть поле без технического имени.");
                    return null;
                }

                if (string.IsNullOrWhiteSpace(field.DisplayName))
                {
                    SetError($"В блоке «{block.Name}» есть поле без отображаемого названия.");
                    return null;
                }

                var norm = BuildNorm(field);

                if (field.HasNorm && norm == null)
                    return null;

                blockDto.Fields.Add(new TemplateFieldDto
                {
                    Id = field.Id == Guid.Empty ? Guid.NewGuid() : field.Id,
                    FieldName = field.FieldName.Trim(),
                    DisplayName = field.DisplayName.Trim(),
                    Position = field.Position,
                    Phrases = SplitPhrases(field.PhrasesText),
                    Type = MapFieldType(field.TypeText),
                    Role = MapFieldRole(field.RoleText),
                    Norm = norm
                });
            }

            result.Add(blockDto);
        }

        return result;
    }

    private FieldNormDto? BuildNorm(EditableTemplateFieldItem field)
    {
        if (!field.HasNorm)
            return null;

        decimal? min = null;
        decimal? max = null;

        if (!string.IsNullOrWhiteSpace(field.NormMin))
        {
            if (!TryParseDecimal(field.NormMin, out var value))
            {
                SetError($"Некорректное минимальное значение нормы у поля «{field.DisplayName}».");
                return null;
            }

            min = value;
        }

        if (!string.IsNullOrWhiteSpace(field.NormMax))
        {
            if (!TryParseDecimal(field.NormMax, out var value))
            {
                SetError($"Некорректное максимальное значение нормы у поля «{field.DisplayName}».");
                return null;
            }

            max = value;
        }

        return new FieldNormDto
        {
            Min = min,
            Max = max,
            Unit = string.IsNullOrWhiteSpace(field.NormUnit) ? null : field.NormUnit.Trim(),
            NormalText = string.IsNullOrWhiteSpace(field.NormNormalText) ? null : field.NormNormalText.Trim()
        };
    }

    private async Task DeleteTemplateAsync(TemplateAdminListItem? item)
    {
        ClearError();

        if (item == null)
            return;

        var command = new DeleteTemplateCommand
        {
            TemplateId = item.Id,
            ExpectedVersion = item.Version
        };

        var result = await _templateService.DeleteAsync(command);

        if (result.IsSuccess)
            RefreshLaterIfCurrent(SearchTemplatesAsync);
        else
            SetError(result.ErrorMessage);
    }

    private void RecalculatePositions()
    {
        for (var i = 0; i < EditableBlocks.Count; i++)
        {
            var block = EditableBlocks[i];
            block.Position = i + 1;

            for (var j = 0; j < block.Fields.Count; j++)
                block.Fields[j].Position = j + 1;
        }
    }

    private static List<string> SplitPhrases(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        return text
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string JoinPhrases(List<string>? phrases)
    {
        return phrases == null || phrases.Count == 0
            ? string.Empty
            : string.Join(", ", phrases);
    }

    private static bool TryParseDecimal(string text, out decimal value)
    {
        return decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out value)
               || decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }

    private static TemplateFieldType? MapNullableFieldType(string? text)
    {
        return text switch
        {
            "Текст" => TemplateFieldType.Text,
            "Число" => TemplateFieldType.Number,
            "Число с единицей измерения" => TemplateFieldType.NumberWithUnit,
            _ => null
        };
    }

    private static TemplateFieldType MapFieldType(string text)
    {
        return MapNullableFieldType(text) ?? TemplateFieldType.Text;
    }

    private static string GetFieldTypeText(TemplateFieldType type)
    {
        return type switch
        {
            TemplateFieldType.Text => "Текст",
            TemplateFieldType.Number => "Число",
            TemplateFieldType.NumberWithUnit => "Число с единицей измерения",
            _ => type.ToString()
        };
    }

    private static TemplateFieldRole? MapNullableFieldRole(string? text)
    {
        return text switch
        {
            "Обычное поле" => TemplateFieldRole.Regular,
            "Описание исследования" => TemplateFieldRole.Description,
            "Заключение исследования" => TemplateFieldRole.Conclusion,
            _ => null
        };
    }

    private static TemplateFieldRole MapFieldRole(string text)
    {
        return MapNullableFieldRole(text) ?? TemplateFieldRole.Regular;
    }

    private static string GetFieldRoleText(TemplateFieldRole role)
    {
        return role switch
        {
            TemplateFieldRole.Regular => "Обычное поле",
            TemplateFieldRole.Description => "Описание исследования",
            TemplateFieldRole.Conclusion => "Заключение исследования",
            _ => role.ToString()
        };
    }
}
