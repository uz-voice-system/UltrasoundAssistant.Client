using System.Net.Http.Json;
using UltrasoundAssistant.DoctorClient.Models.Auth;
using UltrasoundAssistant.DoctorClient.Models.Common;

namespace UltrasoundAssistant.DoctorClient.Services;

public class AuthApiService : ApiServiceBase
{
    private readonly HttpClient _httpClient;

    public AuthApiService(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("UnauthorizedClient");
    }

    public async Task<QueryResult<LoginResult>> LoginAsync(
        LoginRequest request,
        CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", request, ct);

        if (!response.IsSuccessStatusCode)
            return await ReadQueryResultAsync<LoginResult>(response, "Неверный логин или пароль.");

        var data = await response.Content.ReadFromJsonAsync<LoginResult>(cancellationToken: ct);

        return data is null
            ? QueryResult<LoginResult>.Failure("Сервер вернул пустой ответ.")
            : QueryResult<LoginResult>.Success(data);
    }
}
