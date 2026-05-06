using System.Net.Http.Json;
using UltrasoundAssistant.DoctorClient.Models.Common;
using UltrasoundAssistant.DoctorClient.Models.Commands.Schedules;
using UltrasoundAssistant.DoctorClient.Models.Reads.Schedules.Details;
using UltrasoundAssistant.DoctorClient.Models.Reads.Schedules.Search;

namespace UltrasoundAssistant.DoctorClient.Services;

public class ScheduleApiService : ApiServiceBase
{
    private readonly HttpClient _httpClient;

    public ScheduleApiService(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("AuthorizedClient");
    }

    public async Task<QueryResult<UserScheduleDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"api/schedules/{id}", ct);
        return await ReadQueryResultAsync<UserScheduleDto>(response, "Расписание не найдено.");
    }

    public async Task<QueryResult<UserScheduleDto>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"api/schedules/by-user/{userId}", ct);
        return await ReadQueryResultAsync<UserScheduleDto>(response, "Расписание пользователя не найдено.");
    }

    public async Task<QueryResult<List<UserScheduleSummaryDto>>> SearchAsync(
        UserScheduleSearchRequest filter,
        CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/schedules/search", filter, ct);
        return await ReadQueryResultAsync<List<UserScheduleSummaryDto>>(response, "Расписания не найдены.");
    }

    public Task<QueryResult<List<UserScheduleSummaryDto>>> GetAllAsync(CancellationToken ct = default)
    {
        return SearchAsync(new UserScheduleSearchRequest(), ct);
    }

    public async Task<CommandResult> UpdateAsync(UpdateUserScheduleCommand command, CancellationToken ct = default)
    {
        var response = await _httpClient.PutAsJsonAsync("api/schedules", command, ct);
        return await ReadCommandResultAsync(response, "Не удалось обновить расписание.");
    }
}
