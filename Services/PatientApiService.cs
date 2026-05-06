using System.Net.Http.Json;
using UltrasoundAssistant.DoctorClient.Models.Common;
using UltrasoundAssistant.DoctorClient.Models.Commands.Patients;
using UltrasoundAssistant.DoctorClient.Models.Reads.Patients.Details;
using UltrasoundAssistant.DoctorClient.Models.Reads.Patients.Search;

namespace UltrasoundAssistant.DoctorClient.Services;

public class PatientApiService : ApiServiceBase
{
    private readonly HttpClient _httpClient;

    public PatientApiService(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("AuthorizedClient");
    }

    public async Task<QueryResult<PatientDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"api/patients/{id}", ct);
        return await ReadQueryResultAsync<PatientDto>(response, "Пациент не найден.");
    }

    public async Task<QueryResult<List<PatientSummaryDto>>> SearchAsync(
        PatientSearchRequest filter,
        CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/patients/search", filter, ct);
        return await ReadQueryResultAsync<List<PatientSummaryDto>>(response, "Пациенты не найдены.");
    }

    public Task<QueryResult<List<PatientSummaryDto>>> GetAllAsync(CancellationToken ct = default)
    {
        return SearchAsync(new PatientSearchRequest(), ct);
    }

    public async Task<CommandResult> CreateAsync(CreatePatientCommand command, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/patients", command, ct);
        return await ReadCommandResultAsync(response, "Не удалось создать пациента.");
    }

    public async Task<CommandResult> UpdateAsync(UpdatePatientCommand command, CancellationToken ct = default)
    {
        var response = await _httpClient.PutAsJsonAsync("api/patients", command, ct);
        return await ReadCommandResultAsync(response, "Не удалось обновить пациента.");
    }

    public async Task<CommandResult> DeleteAsync(DeletePatientCommand command, CancellationToken ct = default)
    {
        var response = await _httpClient.DeleteAsJsonAsync("api/patients", command, ct);
        return await ReadCommandResultAsync(response, "Не удалось удалить пациента.");
    }
}
