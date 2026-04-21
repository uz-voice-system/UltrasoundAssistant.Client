using System.Net;
using System.Net.Http.Json;
using UltrasoundAssistant.DoctorClient.Models;
using UltrasoundAssistant.DoctorClient.Models.Common;

namespace UltrasoundAssistant.DoctorClient.Services;

public class AuthApiService : ApiServiceBase
{
    private readonly HttpClient _httpClient;

    public AuthApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<QueryResult<LoginResponse>> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return QueryResult<LoginResponse>.Failure("Неверный логин или пароль.");

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var badRequestText = await response.Content.ReadAsStringAsync();
                return QueryResult<LoginResponse>.Failure(
                    string.IsNullOrWhiteSpace(badRequestText)
                        ? "Некорректные данные для входа."
                        : badRequestText);
            }

            return await ReadQueryResultAsync<LoginResponse>(response, "Пользователь не найден.");
        }
        catch (Exception ex)
        {
            return QueryResult<LoginResponse>.Failure($"Ошибка соединения: {ex.Message}");
        }
    }
}