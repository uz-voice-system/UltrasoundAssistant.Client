using UltrasoundAssistant.DoctorClient.Models.Enums;

namespace UltrasoundAssistant.DoctorClient.Models.Reads.Users.Search;

/// <summary>
/// Фильтр поиска пользователей.
/// </summary>
public sealed class UserSearchRequest
{
    /// <summary>
    /// Общая строка поиска.
    /// Используется для поиска по логину и ФИО.
    /// </summary>
    public string? SearchText { get; set; }

    /// <summary>
    /// Логин пользователя.
    /// </summary>
    public string? Login { get; set; }

    /// <summary>
    /// ФИО пользователя.
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Роль пользователя.
    /// Например, врач, регистратор или администратор.
    /// </summary>
    public UserRole? Role { get; set; }

    /// <summary>
    /// Признак активности пользователя.
    /// Если не указан, возвращаются все подходящие пользователи.
    /// </summary>
    public bool? IsActive { get; set; }
}
