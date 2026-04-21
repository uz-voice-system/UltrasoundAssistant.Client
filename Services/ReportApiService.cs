using System.Net.Http.Json;
using UltrasoundAssistant.DoctorClient.Models.Commands.Report;
using UltrasoundAssistant.DoctorClient.Models.Common;
using UltrasoundAssistant.DoctorClient.Models.Read.Report;

namespace UltrasoundAssistant.DoctorClient.Services;

public class ReportApiService : ApiServiceBase
{
    private readonly HttpClient _httpClient;

    public ReportApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<QueryResult<List<ReportDto>>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync("api/reports");
        return await ReadQueryResultAsync<List<ReportDto>>(response, "Отчёты не найдены.");
    }

    public async Task<QueryResult<ReportDto>> GetByIdAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"api/reports/{id}");
        return await ReadQueryResultAsync<ReportDto>(response, "Отчёт не найден.");
    }

    public async Task<CommandResult> CreateAsync(CreateReportCommand command)
    {
        var response = await _httpClient.PostAsJsonAsync("api/reports", command);
        return await ReadCommandResultAsync(response, "Не удалось создать отчёт.");
    }

    public async Task<CommandResult> UpdateFieldAsync(UpdateReportFieldCommand command)
    {
        var response = await _httpClient.PostAsJsonAsync("api/reports/field", command);
        return await ReadCommandResultAsync(response, "Не удалось обновить поле отчёта.");
    }

    public async Task<CommandResult> CompleteAsync(CompleteReportCommand command)
    {
        var response = await _httpClient.PostAsJsonAsync("api/reports/complete", command);
        return await ReadCommandResultAsync(response, "Не удалось завершить отчёт.");
    }

    public async Task<CommandResult> DeleteAsync(DeleteReportCommand command)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, "api/reports")
        {
            Content = JsonContent.Create(command)
        };

        var response = await _httpClient.SendAsync(request);
        return await ReadCommandResultAsync(response, "Не удалось удалить отчёт.");
    }
}