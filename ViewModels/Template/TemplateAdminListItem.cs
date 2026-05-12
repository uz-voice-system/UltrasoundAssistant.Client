using Avalonia.Media;
using UltrasoundAssistant.DoctorClient.Models.Reads.Templates.Admin;
using UltrasoundAssistant.DoctorClient.Models.Reads.Templates.Details;

namespace UltrasoundAssistant.DoctorClient.ViewModels.Template;

public sealed class TemplateAdminListItem
{
    public TemplateAdminSearchResultDto Source { get; }

    public TemplateDto Template => Source.Template;

    public Guid Id => Template.Id;

    public string Name => string.IsNullOrWhiteSpace(Template.Name)
        ? "Без названия"
        : Template.Name;

    public int Version => Template.Version;

    public bool IsDeleted => Template.IsDeleted;

    public int DurationMinutes => Template.DefaultAppointmentDurationMinutes;

    public int BlocksCount => Template.Blocks?.Count ?? 0;

    public int FieldsCount => Template.Blocks?.Sum(x => x.Fields?.Count ?? 0) ?? 0;

    public int MatchesCount => Source.Matches?.Count ?? 0;

    public string DurationText => DurationMinutes <= 0
        ? "Длительность не указана"
        : $"{DurationMinutes} мин.";

    public string StructureText => $"{BlocksCount} блок(ов), {FieldsCount} пол(ей)";

    public string MatchesText => MatchesCount == 0
        ? "Совпадений по расширенному поиску нет"
        : $"Совпадений: {MatchesCount}";

    public string StatusText => IsDeleted ? "Удалён" : "Активен";

    public IBrush CardBackground => IsDeleted
        ? Hex("#FFF5F5")
        : Brushes.White;

    public IBrush CardBorderBrush => IsDeleted
        ? Hex("#E74C3C")
        : Hex("#DDDDDD");

    public IBrush StatusBackground => IsDeleted
        ? Hex("#FDE2E2")
        : Hex("#DFF5E1");

    public IBrush StatusForeground => IsDeleted
        ? Hex("#B00020")
        : Hex("#1E7E34");

    private static IBrush Hex(string color)
    {
        return new SolidColorBrush(Color.Parse(color));
    }

    public TemplateAdminListItem(TemplateAdminSearchResultDto source)
    {
        Source = source;
    }
}
