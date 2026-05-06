namespace UltrasoundAssistant.DoctorClient.Models.Commands.Users;

/// <summary>
/// Команда деактивации пользователя
/// </summary>
public sealed class DeactivateUserCommand
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Ожидаемая версия агрегата
    /// </summary>
    public int ExpectedVersion { get; set; }
}
