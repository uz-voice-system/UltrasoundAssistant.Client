using UltrasoundAssistant.DoctorClient.Models.Entity.Users;
using UltrasoundAssistant.DoctorClient.Models.Enums;

namespace UltrasoundAssistant.DoctorClient.Models.Commands.Users;

/// <summary>
/// Команда создания пользователя
/// </summary>
public sealed class CreateUserCommand
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
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// ФИО пользователя
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Роль пользователя
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Профиль врача
    /// </summary>
    public DoctorProfileDto? DoctorProfile { get; set; }
}
