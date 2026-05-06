using System.Net.Http.Json;

namespace UltrasoundAssistant.DoctorClient.Services;

internal static class HttpClientJsonExtensions
{
    public static Task<HttpResponseMessage> DeleteAsJsonAsync<T>(
        this HttpClient client,
        string requestUri,
        T value,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsJsonAsync(HttpMethod.Delete, requestUri, value, cancellationToken);
    }

    public static Task<HttpResponseMessage> PatchAsJsonAsync<T>(
        this HttpClient client,
        string requestUri,
        T value,
        CancellationToken cancellationToken = default)
    {
        return client.SendAsJsonAsync(HttpMethod.Patch, requestUri, value, cancellationToken);
    }

    private static async Task<HttpResponseMessage> SendAsJsonAsync<T>(
        this HttpClient client,
        HttpMethod method,
        string requestUri,
        T value,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, requestUri)
        {
            Content = JsonContent.Create(value)
        };

        return await client.SendAsync(request, cancellationToken);
    }
}
