using Avalonia.Media;
using UltrasoundAssistant.DoctorClient.Models.Reads.Patients.Search;

namespace UltrasoundAssistant.DoctorClient.ViewModels.Patient;

public sealed class PatientAdminListItem
{
    public PatientSummaryDto Source { get; }

    public Guid Id => Source.Id;

    public string FullName => Source.FullName;

    public DateTime BirthDate => Source.BirthDate;

    public string? Gender => Source.Gender;

    public string? PhoneNumber => Source.PhoneNumber;

    public bool IsDeleted => Source.IsDeleted;

    public int Version => Source.Version;

    public string BirthDateText => BirthDate == default
        ? "Дата рождения не указана"
        : BirthDate.ToString("dd.MM.yyyy");

    public string GenderText => string.IsNullOrWhiteSpace(Gender)
        ? "Пол не указан"
        : Gender;

    public string PhoneText => string.IsNullOrWhiteSpace(PhoneNumber)
        ? "Телефон не указан"
        : PhoneNumber;

    public string StatusText => IsDeleted ? "Удалён" : "Активен";

    public string AgeText
    {
        get
        {
            if (BirthDate == default)
                return string.Empty;

            var today = DateTime.Today;
            var age = today.Year - BirthDate.Year;

            if (BirthDate.Date > today.AddYears(-age))
                age--;

            if (age < 0)
                return string.Empty;

            return $"{age} лет";
        }
    }

    public IBrush CardBackground => IsDeleted
        ? Brush.Parse("#FFF5F5")
        : Brushes.White;

    public IBrush CardBorderBrush => IsDeleted
        ? Brush.Parse("#E74C3C")
        : Brush.Parse("#DDDDDD");

    public IBrush StatusBackground => IsDeleted
        ? Brush.Parse("#FDE2E2")
        : Brush.Parse("#DFF5E1");

    public IBrush StatusForeground => IsDeleted
        ? Brush.Parse("#B00020")
        : Brush.Parse("#1E7E34");

    public PatientAdminListItem(PatientSummaryDto source)
    {
        Source = source;
    }
}
