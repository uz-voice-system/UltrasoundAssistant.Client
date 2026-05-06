namespace UltrasoundAssistant.DoctorClient.Models.Entity.Users;

/// <summary>
/// Профиль врача.
/// </summary>
public sealed class DoctorProfileDto
{
    /// <summary>
    /// Идентификатор пользователя-врача.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Специализация врача.
    /// </summary>
    public string? Specialization { get; set; }

    /// <summary>
    /// Кабинет врача.
    /// </summary>
    public string? Cabinet { get; set; }

    /// <summary>
    /// Внутренний телефон.
    /// </summary>
    public string? PhoneExtension { get; set; }
}
