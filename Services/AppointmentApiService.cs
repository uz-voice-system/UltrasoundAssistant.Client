using System.Net.Http.Json;
using UltrasoundAssistant.DoctorClient.Models.Common;
using UltrasoundAssistant.DoctorClient.Models.Commands.Appointments;
using UltrasoundAssistant.DoctorClient.Models.Reads.Appointments.Details;
using UltrasoundAssistant.DoctorClient.Models.Reads.Appointments.Search;

namespace UltrasoundAssistant.DoctorClient.Services;

public class AppointmentApiService : ApiServiceBase
{
    private readonly HttpClient _httpClient;

    public AppointmentApiService(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("AuthorizedClient");
    }

    public async Task<QueryResult<AppointmentDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"api/appointments/{id}", ct);
        return await ReadQueryResultAsync<AppointmentDto>(response, "Приём не найден.");
    }

    public async Task<QueryResult<List<AppointmentSummaryDto>>> SearchAsync(
        AppointmentSearchRequest filter,
        CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/appointments/search", filter, ct);
        return await ReadQueryResultAsync<List<AppointmentSummaryDto>>(response, "Приёмы не найдены.");
    }

    public Task<QueryResult<List<AppointmentSummaryDto>>> GetAllAsync(CancellationToken ct = default)
    {
        return SearchAsync(new AppointmentSearchRequest(), ct);
    }

    public async Task<CommandResult> CreateAsync(CreateAppointmentCommand command, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/appointments", command, ct);
        return await ReadCommandResultAsync(response, "Не удалось создать приём.");
    }

    public async Task<CommandResult> UpdateAsync(UpdateAppointmentCommand command, CancellationToken ct = default)
    {
        var response = await _httpClient.PutAsJsonAsync("api/appointments", command, ct);
        return await ReadCommandResultAsync(response, "Не удалось обновить приём.");
    }

    public async Task<CommandResult> DeleteAsync(DeleteAppointmentCommand command, CancellationToken ct = default)
    {
        var response = await _httpClient.DeleteAsJsonAsync("api/appointments", command, ct);
        return await ReadCommandResultAsync(response, "Не удалось удалить приём.");
    }
}
