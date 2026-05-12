using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using UltrasoundAssistant.DoctorClient.Models.Common;

namespace UltrasoundAssistant.DoctorClient.Services;

public abstract class ApiServiceBase
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    protected static async Task<QueryResult<T>> ReadQueryResultAsync<T>(HttpResponseMessage response, string notFoundMessage)
    {
        try
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
                return QueryResult<T>.Failure(notFoundMessage);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return QueryResult<T>.Failure(
                    string.IsNullOrWhiteSpace(error)
                        ? $"Ошибка запроса: {(int)response.StatusCode}"
                        : error);
            }

            var data = await response.Content.ReadFromJsonAsync<T>(JsonOptions);

            return data == null
                ? QueryResult<T>.Failure("Сервер вернул пустой ответ.")
                : QueryResult<T>.Success(data);
        }
        catch (Exception ex)
        {
            return QueryResult<T>.Failure($"Ошибка соединения: {ex.Message}");
        }
    }

    protected static async Task<CommandResult> ReadCommandResultAsync(HttpResponseMessage response, string defaultErrorMessage)
    {
        try
        {
            if (response.IsSuccessStatusCode)
                return CommandResult.Success();

            var error = await response.Content.ReadAsStringAsync();

            return CommandResult.Failure(
                string.IsNullOrWhiteSpace(error)
                    ? defaultErrorMessage
                    : error);
        }
        catch (Exception ex)
        {
            return CommandResult.Failure($"Ошибка соединения: {ex.Message}");
        }
    }
}