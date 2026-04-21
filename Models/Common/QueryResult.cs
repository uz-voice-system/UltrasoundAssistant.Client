namespace UltrasoundAssistant.DoctorClient.Models.Common;

public class QueryResult<T>
{
    public T? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public bool IsSuccess => ErrorMessage == null;

    public static QueryResult<T> Success(T data) => new()
    {
        Data = data
    };

    public static QueryResult<T> Failure(string errorMessage) => new()
    {
        ErrorMessage = errorMessage
    };
}