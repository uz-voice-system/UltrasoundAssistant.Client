using UltrasoundAssistant.DoctorClient.Models.Entity.Users;
using UltrasoundAssistant.DoctorClient.Models.Enums;

namespace UltrasoundAssistant.DoctorClient.Models.Reads.Users.Details;

/// <summary>
/// Полная информация о пользователе.
/// </summary>
public sealed class UserDto
{
    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Логин пользователя.
    /// </summary>
    public string Login { get; set; } = string.Empty;

    /// <summary>
    /// ФИО пользователя.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Роль пользователя.
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Признак активности пользователя.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Версия агрегата.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Профиль врача.
    /// Заполняется только для пользователей с ролью врача.
    /// </summary>
    public DoctorProfileDto? DoctorProfile { get; set; }
}