namespace UltrasoundAssistant.DoctorClient.Models.Commands.Users;

/// <summary>
/// Команда активации пользователя
/// </summary>
public sealed class ActivateUserCommand
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
