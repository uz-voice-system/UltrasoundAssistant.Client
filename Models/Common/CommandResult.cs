namespace UltrasoundAssistant.DoctorClient.Models.Common;

public class CommandResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }

    public static CommandResult Success() => new()
    {
        IsSuccess = true
    };

    public static CommandResult Failure(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };
}