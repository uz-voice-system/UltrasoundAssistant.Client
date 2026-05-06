using System.Net.Http.Json;
using UltrasoundAssistant.DoctorClient.Models.Common;
using UltrasoundAssistant.DoctorClient.Models.Commands.Users;
using UltrasoundAssistant.DoctorClient.Models.Reads.Users.Details;
using UltrasoundAssistant.DoctorClient.Models.Reads.Users.Search;

namespace UltrasoundAssistant.DoctorClient.Services;

public class UserApiService : ApiServiceBase
{
    private readonly HttpClient _httpClient;

    public UserApiService(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("AuthorizedClient");
    }

    public async Task<QueryResult<UserDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"api/users/{id}", ct);
        return await ReadQueryResultAsync<UserDto>(response, "Пользователь не найден.");
    }

    public async Task<QueryResult<List<UserSummaryDto>>> SearchAsync(
        UserSearchRequest filter,
        CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/users/search", filter, ct);
        return await ReadQueryResultAsync<List<UserSummaryDto>>(response, "Пользователи не найдены.");
    }

    public Task<QueryResult<List<UserSummaryDto>>> GetAllAsync(CancellationToken ct = default)
    {
        return SearchAsync(new UserSearchRequest(), ct);
    }

    public async Task<CommandResult> CreateAsync(CreateUserCommand command, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/users", command, ct);
        return await ReadCommandResultAsync(response, "Не удалось создать пользователя.");
    }

    public async Task<CommandResult> UpdateAsync(UpdateUserCommand command, CancellationToken ct = default)
    {
        var response = await _httpClient.PutAsJsonAsync("api/users", command, ct);
        return await ReadCommandResultAsync(response, "Не удалось обновить пользователя.");
    }

    public async Task<CommandResult> ActivateAsync(ActivateUserCommand command, CancellationToken ct = default)
    {
        var response = await _httpClient.PatchAsJsonAsync("api/users/activate", command, ct);
        return await ReadCommandResultAsync(response, "Не удалось активировать пользователя.");
    }

    public async Task<CommandResult> DeactivateAsync(DeactivateUserCommand command, CancellationToken ct = default)
    {
        var response = await _httpClient.PatchAsJsonAsync("api/users/deactivate", command, ct);
        return await ReadCommandResultAsync(response, "Не удалось деактивировать пользователя.");
    }
}
