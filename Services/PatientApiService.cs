using System.Net.Http.Json;
using UltrasoundAssistant.DoctorClient.Models.Commands.Patient;
using UltrasoundAssistant.DoctorClient.Models.Common;
using UltrasoundAssistant.DoctorClient.Models.Read.Patient;

namespace UltrasoundAssistant.DoctorClient.Services;

public class PatientApiService : ApiServiceBase
{
    private readonly HttpClient _httpClient;

    public PatientApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<QueryResult<List<PatientDto>>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync("api/patients");
        return await ReadQueryResultAsync<List<PatientDto>>(response, "Пациенты не найдены.");
    }

    public async Task<QueryResult<PatientDto>> GetByIdAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"api/patients/{id}");
        return await ReadQueryResultAsync<PatientDto>(response, "Пациент не найден.");
    }

    public async Task<CommandResult> CreateAsync(CreatePatientCommand command)
    {
        var response = await _httpClient.PostAsJsonAsync("api/patients", command);
        return await ReadCommandResultAsync(response, "Не удалось создать пациента.");
    }

    public async Task<CommandResult> UpdateAsync(UpdatePatientCommand command)
    {
        var response = await _httpClient.PutAsJsonAsync("api/patients", command);
        return await ReadCommandResultAsync(response, "Не удалось обновить пациента.");
    }

    public async Task<CommandResult> DeactivateAsync(DeactivatePatientCommand command)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, "api/patients")
        {
            Content = JsonContent.Create(command)
        };

        var response = await _httpClient.SendAsync(request);
        return await ReadCommandResultAsync(response, "Не удалось деактивировать пациента.");
    }

    public async Task<QueryResult<List<PatientDto>>> SearchByFullNameAsync(string fullName)
    {
        var result = await GetAllAsync();

        if (!result.IsSuccess || result.Data == null)
            return QueryResult<List<PatientDto>>.Failure(result.ErrorMessage ?? "Не удалось загрузить пациентов.");

        var filtered = result.Data
            .Where(x => !string.IsNullOrWhiteSpace(x.FullName) && x.FullName.Contains(fullName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return QueryResult<List<PatientDto>>.Success(filtered);
    }
}