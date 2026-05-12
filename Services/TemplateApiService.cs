using System.Net.Http.Json;
using UltrasoundAssistant.DoctorClient.Models.Common;
using UltrasoundAssistant.DoctorClient.Models.Commands.Templates;
using UltrasoundAssistant.DoctorClient.Models.Reads.Templates.Admin;
using UltrasoundAssistant.DoctorClient.Models.Reads.Templates.Details;
using UltrasoundAssistant.DoctorClient.Models.Reads.Templates.Search;

namespace UltrasoundAssistant.DoctorClient.Services;

public class TemplateApiService : ApiServiceBase
{
    private readonly HttpClient _httpClient;

    public TemplateApiService(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("AuthorizedClient");
    }

    public async Task<QueryResult<TemplateDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"api/templates/{id}", ct);
        return await ReadQueryResultAsync<TemplateDto>(response, "Шаблон не найден.");
    }

    public async Task<QueryResult<List<TemplateSummaryDto>>> SearchForDoctorAsync(TemplateSearchRequest filter, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/templates/search", filter, JsonOptions, ct);
        return await ReadQueryResultAsync<List<TemplateSummaryDto>>(response, "Шаблоны не найдены.");
    }

    public async Task<QueryResult<List<TemplateAdminSearchResultDto>>> SearchForAdminAsync(TemplateAdminSearchRequest filter, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/templates/search-admin", filter, JsonOptions, ct);
        return await ReadQueryResultAsync<List<TemplateAdminSearchResultDto>>(response, "Шаблоны не найдены.");
    }

    public Task<QueryResult<List<TemplateSummaryDto>>> GetAllForDoctorAsync(CancellationToken ct = default)
    {
        return SearchForDoctorAsync(new TemplateSearchRequest(), ct);
    }

    public Task<QueryResult<List<TemplateAdminSearchResultDto>>> GetAllForAdminAsync(CancellationToken ct = default)
    {
        return SearchForAdminAsync(new TemplateAdminSearchRequest(), ct);
    }

    public async Task<CommandResult> CreateAsync(CreateTemplateCommand command, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/templates", command, JsonOptions, ct);
        return await ReadCommandResultAsync(response, "Не удалось создать шаблон.");
    }

    public async Task<CommandResult> UpdateAsync(UpdateTemplateCommand command, CancellationToken ct = default)
    {
        var response = await _httpClient.PutAsJsonAsync("api/templates", command, JsonOptions, ct);
        return await ReadCommandResultAsync(response, "Не удалось обновить шаблон.");
    }

    public async Task<CommandResult> DeleteAsync(DeleteTemplateCommand command, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, "api/templates")
        {
            Content = JsonContent.Create(command, options: JsonOptions)
        };

        var response = await _httpClient.SendAsync(request, ct);
        return await ReadCommandResultAsync(response, "Не удалось удалить шаблон.");
    }
}