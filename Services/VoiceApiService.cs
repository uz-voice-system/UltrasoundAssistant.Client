using System.Net.Http.Json;
using UltrasoundAssistant.DoctorClient.Models.Common;
using UltrasoundAssistant.DoctorClient.Models.Voice;

namespace UltrasoundAssistant.DoctorClient.Services;

public class VoiceApiService : ApiServiceBase
{
    private readonly HttpClient _httpClient;

    public VoiceApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<QueryResult<VoiceProcessResult>> ProcessAsync(VoiceProcessRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/voice/process", request);
        return await ReadQueryResultAsync<VoiceProcessResult>(response, "Не удалось обработать голосовой ввод.");
    }
}