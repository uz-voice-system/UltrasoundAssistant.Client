using UltrasoundAssistant.DoctorClient.Models.Entity.Users;
using UltrasoundAssistant.DoctorClient.Models.Enums;

namespace UltrasoundAssistant.DoctorClient.Models.Commands.Users;

/// <summary>
/// Команда обновления пользователя
/// </summary>
public sealed class UpdateUserCommand
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Логин пользователя
    /// </summary>
    public string Login { get; set; } = string.Empty;

    /// <summary>
    /// Пароль пользователя
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// ФИО пользователя
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Роль пользователя
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Признак активности пользователя
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Профиль врача
    /// </summary>
    public DoctorProfileDto? DoctorProfile { get; set; }

    /// <summary>
    /// Ожидаемая версия агрегата
    /// </summary>
    public int ExpectedVersion { get; set; }
}
