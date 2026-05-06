using System.Net.Http.Json;
using UltrasoundAssistant.DoctorClient.Models.Common;
using UltrasoundAssistant.DoctorClient.Models.Voice;

namespace UltrasoundAssistant.DoctorClient.Services;

public class VoiceApiService : ApiServiceBase
{
    private readonly HttpClient _httpClient;

    public VoiceApiService(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("AuthorizedClient");
    }

    public async Task<QueryResult<VoiceProcessResult>> ProcessAsync(VoiceProcessRequest request, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/voice/process", request, ct);
        return await ReadQueryResultAsync<VoiceProcessResult>(response, "Не удалось обработать голосовой ввод.");
    }
}
