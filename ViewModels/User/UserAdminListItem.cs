using Avalonia.Media;
using UltrasoundAssistant.DoctorClient.Models.Enums;
using UltrasoundAssistant.DoctorClient.Models.Reads.Users.Search;

namespace UltrasoundAssistant.DoctorClient.ViewModels.User;

public sealed class UserAdminListItem
{
    public UserSummaryDto Source { get; }
    public Guid CurrentUserId { get; }

    public Guid Id => Source.Id;
    public string Login => Source.Login;
    public string FullName => Source.FullName;
    public UserRole Role => Source.Role;
    public bool IsActive => Source.IsActive;
    public string? Cabinet => Source.Cabinet;
    public string? Specialization => Source.Specialization;
    public int Version => Source.Version;

    public bool IsCurrentUser => Id == CurrentUserId;

    public string RoleText => Role switch
    {
        UserRole.Doctor => "Врач",
        UserRole.Admin => "Администратор",
        UserRole.Registrar => "Регистратор",
        _ => Role.ToString()
    };

    public string StatusText => IsActive ? "Активен" : "Неактивен";

    public string ActivationButtonText => IsActive ? "Деактивировать" : "Активировать";

    public string CurrentUserText => IsCurrentUser ? "Это вы" : string.Empty;

    public string DoctorInfoText
    {
        get
        {
            if (Role != UserRole.Doctor)
                return "—";

            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(Specialization))
                parts.Add(Specialization);

            if (!string.IsNullOrWhiteSpace(Cabinet))
                parts.Add($"каб. {Cabinet}");

            return parts.Count == 0
                ? "Профиль врача не заполнен"
                : string.Join(", ", parts);
        }
    }

    public IBrush CardBackground => IsCurrentUser
        ? Brush.Parse("#FFF8E1")
        : Brushes.White;

    public IBrush CardBorderBrush => IsCurrentUser
        ? Brush.Parse("#F2C94C")
        : Brush.Parse("#DDDDDD");

    public IBrush StatusBackground => IsActive
        ? Brush.Parse("#DFF5E1")
        : Brush.Parse("#FDE2E2");

    public IBrush StatusForeground => IsActive
        ? Brush.Parse("#1E7E34")
        : Brush.Parse("#B00020");

    public IBrush RoleBackground => Role switch
    {
        UserRole.Doctor => Brush.Parse("#E8F1FF"),
        UserRole.Admin => Brush.Parse("#F3E8FF"),
        UserRole.Registrar => Brush.Parse("#E8FFF3"),
        _ => Brush.Parse("#EEF2F7")
    };

    public IBrush RoleForeground => Role switch
    {
        UserRole.Doctor => Brush.Parse("#2457A6"),
        UserRole.Admin => Brush.Parse("#6F2DA8"),
        UserRole.Registrar => Brush.Parse("#1E7E50"),
        _ => Brush.Parse("#444444")
    };

    public IBrush ActivityButtonBackground => IsActive
        ? Brush.Parse("#E74C3C")
        : Brush.Parse("#2ECC71");

    public UserAdminListItem(UserSummaryDto source, Guid currentUserId)
    {
        Source = source;
        CurrentUserId = currentUserId;
    }
}
