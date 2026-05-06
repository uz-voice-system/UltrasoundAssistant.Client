using UltrasoundAssistant.DoctorClient.Models.Enums;

namespace UltrasoundAssistant.DoctorClient.Models.Reads.Users.Search;

/// <summary>
/// Краткая информация о пользователе.
/// </summary>
public sealed class UserSummaryDto
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
    /// Признак активности.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Кабинет врача.
    /// Заполняется только для врача.
    /// </summary>
    public string? Cabinet { get; set; }

    /// <summary>
    /// Специализация врача.
    /// Заполняется только для врача.
    /// </summary>
    public string? Specialization { get; set; }

    /// <summary>
    /// Версия агрегата.
    /// </summary>
    public int Version { get; set; }
}
