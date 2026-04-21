using System.Net.Http.Json;
using UltrasoundAssistant.DoctorClient.Models.Commands.Template;
using UltrasoundAssistant.DoctorClient.Models.Common;
using UltrasoundAssistant.DoctorClient.Models.Read.Template;

namespace UltrasoundAssistant.DoctorClient.Services;

public class TemplateApiService : ApiServiceBase
{
    private readonly HttpClient _httpClient;

    public TemplateApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<QueryResult<List<TemplateDto>>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync("api/templates");
        return await ReadQueryResultAsync<List<TemplateDto>>(response, "Шаблоны не найдены.");
    }

    public async Task<QueryResult<TemplateDto>> GetByIdAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"api/templates/{id}");
        return await ReadQueryResultAsync<TemplateDto>(response, "Шаблон не найден.");
    }

    public async Task<CommandResult> CreateAsync(CreateTemplateCommand command)
    {
        var response = await _httpClient.PostAsJsonAsync("api/templates", command);
        return await ReadCommandResultAsync(response, "Не удалось создать шаблон.");
    }

    public async Task<CommandResult> UpdateAsync(UpdateTemplateCommand command)
    {
        var response = await _httpClient.PutAsJsonAsync("api/templates", command);
        return await ReadCommandResultAsync(response, "Не удалось обновить шаблон.");
    }

    public async Task<CommandResult> DeleteAsync(DeleteTemplateCommand command)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, "api/templates")
        {
            Content = JsonContent.Create(command)
        };

        var response = await _httpClient.SendAsync(request);
        return await ReadCommandResultAsync(response, "Не удалось удалить шаблон.");
    }
}