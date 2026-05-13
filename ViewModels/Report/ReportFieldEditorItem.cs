using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using UltrasoundAssistant.DoctorClient.Models.Entity.Templates;
using UltrasoundAssistant.DoctorClient.Models.Voice;

namespace UltrasoundAssistant.DoctorClient.ViewModels.Report;

public class ReportFieldEditorItem : ObservableObject
{
    private string _blockName = string.Empty;
    public string BlockName
    {
        get => _blockName;
        set => SetProperty(ref _blockName, value);
    }

    private string _fieldName = string.Empty;
    public string FieldName
    {
        get => _fieldName;
        set => SetProperty(ref _fieldName, value);
    }

    private string _displayName = string.Empty;
    public string DisplayName
    {
        get => _displayName;
        set => SetProperty(ref _displayName, value);
    }

    private TemplateFieldRole _role = TemplateFieldRole.Regular;
    public TemplateFieldRole Role
    {
        get => _role;
        set
        {
            if (SetProperty(ref _role, value))
            {
                OnPropertyChanged(nameof(IsLongTextField));
                OnPropertyChanged(nameof(EditorMinHeight));
                OnPropertyChanged(nameof(RoleText));
                OnPropertyChanged(nameof(HasRoleText));
            }
        }
    }

    private string _phrasesText = string.Empty;
    public string PhrasesText
    {
        get => _phrasesText;
        set => SetProperty(ref _phrasesText, value);
    }

    private string _value = string.Empty;
    public string Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    private string _originalValue = string.Empty;
    public string OriginalValue
    {
        get => _originalValue;
        set => SetProperty(ref _originalValue, value);
    }

    private string? _normMessage;
    public string? NormMessage
    {
        get => _normMessage;
        set => SetProperty(ref _normMessage, value);
    }

    private bool _wasRecognized;
    public bool WasRecognized
    {
        get => _wasRecognized;
        set
        {
            if (SetProperty(ref _wasRecognized, value))
                RaiseRecognitionProperties();
        }
    }

    private double? _confidence;
    public double? Confidence
    {
        get => _confidence;
        set
        {
            if (SetProperty(ref _confidence, value))
                RaiseRecognitionProperties();
        }
    }

    private NormStatus _normStatus = NormStatus.Unknown;
    public NormStatus NormStatus
    {
        get => _normStatus;
        set => SetProperty(ref _normStatus, value);
    }

    public bool IsLongTextField => Role is TemplateFieldRole.Description or TemplateFieldRole.Conclusion;

    public double EditorMinHeight => IsLongTextField ? 150 : 42;

    public string RoleText => Role switch
    {
        TemplateFieldRole.Description => "Описание исследования",
        TemplateFieldRole.Conclusion => "Заключение исследования",
        _ => string.Empty
    };

    public bool HasRoleText => !string.IsNullOrWhiteSpace(RoleText);

    public bool HasRecognitionInfo => WasRecognized;

    public string ConfidenceText
    {
        get
        {
            if (!WasRecognized)
                return string.Empty;

            if (Confidence == null)
                return "Распознано, уверенность не указана";

            return $"Уверенность: {Confidence.Value:P0}";
        }
    }

    public string RecognitionText
    {
        get
        {
            if (!WasRecognized)
                return string.Empty;

            if (Confidence == null)
                return "Распознано";

            if (Confidence.Value >= 0.85)
                return "Распознано уверенно";

            if (Confidence.Value >= 0.65)
                return "Проверьте значение";

            return "Низкая уверенность";
        }
    }

    public IBrush RecognitionBackground
    {
        get
        {
            if (!WasRecognized)
                return Brushes.White;

            if (Confidence == null)
                return Hex("#EEF2F7");

            if (Confidence.Value >= 0.85)
                return Hex("#EAF7EA");

            if (Confidence.Value >= 0.65)
                return Hex("#FFF8E1");

            return Hex("#FDEDED");
        }
    }

    public IBrush RecognitionBorderBrush
    {
        get
        {
            if (!WasRecognized)
                return Hex("#DDDDDD");

            if (Confidence == null)
                return Hex("#AAB2BD");

            if (Confidence.Value >= 0.85)
                return Hex("#2ECC71");

            if (Confidence.Value >= 0.65)
                return Hex("#F2C94C");

            return Hex("#E74C3C");
        }
    }

    public IBrush RecognitionForeground
    {
        get
        {
            if (!WasRecognized)
                return Hex("#555555");

            if (Confidence == null)
                return Hex("#444444");

            if (Confidence.Value >= 0.85)
                return Hex("#1E7E34");

            if (Confidence.Value >= 0.65)
                return Hex("#7A5B00");

            return Hex("#B00020");
        }
    }

    public bool IsDirty => !string.Equals(Value?.Trim(), OriginalValue?.Trim(), StringComparison.Ordinal);

    public void ClearRecognition()
    {
        WasRecognized = false;
        Confidence = null;
        NormStatus = NormStatus.Unknown;
    }

    private void RaiseRecognitionProperties()
    {
        OnPropertyChanged(nameof(HasRecognitionInfo));
        OnPropertyChanged(nameof(ConfidenceText));
        OnPropertyChanged(nameof(RecognitionText));
        OnPropertyChanged(nameof(RecognitionBackground));
        OnPropertyChanged(nameof(RecognitionBorderBrush));
        OnPropertyChanged(nameof(RecognitionForeground));
    }

    private static IBrush Hex(string color)
    {
        return new SolidColorBrush(Color.Parse(color));
    }
}
