using UltrasoundAssistant.DoctorClient.Models.Enums;

namespace UltrasoundAssistant.DoctorClient.Models.Auth;

/// <summary>
/// Результат входа пользователя
/// </summary>
public sealed class LoginResult
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
    /// ФИО пользователя
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Роль пользователя
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// JWT токен
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Дата истечения токена
    /// </summary>
    public DateTime ExpiresAtUtc { get; set; }
}
