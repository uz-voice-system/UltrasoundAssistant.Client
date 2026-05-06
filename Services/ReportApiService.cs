using System.Net.Http.Json;
using UltrasoundAssistant.DoctorClient.Models.Common;
using UltrasoundAssistant.DoctorClient.Models.Commands.Reports;
using UltrasoundAssistant.DoctorClient.Models.Reads.Reports.Details;
using UltrasoundAssistant.DoctorClient.Models.Reads.Reports.Search;

namespace UltrasoundAssistant.DoctorClient.Services;

public class ReportApiService : ApiServiceBase
{
    private readonly HttpClient _httpClient;

    public ReportApiService(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("AuthorizedClient");
    }

    public async Task<QueryResult<ReportDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"api/reports/{id}", ct);
        return await ReadQueryResultAsync<ReportDto>(response, "Отчёт не найден.");
    }

    public async Task<QueryResult<ReportDto>> GetByAppointmentIdAsync(
        Guid appointmentId,
        CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"api/reports/by-appointment/{appointmentId}", ct);
        return await ReadQueryResultAsync<ReportDto>(response, "Отчёт по приёму не найден.");
    }

    public async Task<QueryResult<List<ReportSummaryDto>>> SearchAsync(
        ReportSearchRequest filter,
        CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/reports/search", filter, ct);
        return await ReadQueryResultAsync<List<ReportSummaryDto>>(response, "Отчёты не найдены.");
    }

    public Task<QueryResult<List<ReportSummaryDto>>> GetAllAsync(CancellationToken ct = default)
    {
        return SearchAsync(new ReportSearchRequest(), ct);
    }

    public async Task<CommandResult> CreateAsync(CreateReportCommand command, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/reports", command, ct);
        return await ReadCommandResultAsync(response, "Не удалось создать отчёт.");
    }

    public async Task<CommandResult> UpdateAsync(UpdateReportCommand command, CancellationToken ct = default)
    {
        var response = await _httpClient.PutAsJsonAsync("api/reports", command, ct);
        return await ReadCommandResultAsync(response, "Не удалось обновить отчёт.");
    }

    public async Task<CommandResult> DeleteAsync(DeleteReportCommand command, CancellationToken ct = default)
    {
        var response = await _httpClient.DeleteAsJsonAsync("api/reports", command, ct);
        return await ReadCommandResultAsync(response, "Не удалось удалить отчёт.");
    }

    public async Task<QueryResult<ReportPdfFileDto>> GetPdfAsync(Guid reportId, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"api/reports/{reportId}/pdf", ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ReadErrorMessageAsync(response, "Не удалось сформировать PDF отчёта.");

            return QueryResult<ReportPdfFileDto>.Failure(errorMessage);
        }

        var bytes = await response.Content.ReadAsByteArrayAsync(ct);

        var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/pdf";

        var fileName = GetFileName(response.Content.Headers.ContentDisposition) ?? $"report-{reportId:N}.pdf";

        return QueryResult<ReportPdfFileDto>.Success(new ReportPdfFileDto
        {
            FileName = fileName,
            ContentType = contentType,
            Content = bytes
        });
    }

    private static string? GetFileName(
        System.Net.Http.Headers.ContentDispositionHeaderValue? contentDisposition)
    {
        if (contentDisposition is null)
            return null;

        if (!string.IsNullOrWhiteSpace(contentDisposition.FileNameStar))
            return contentDisposition.FileNameStar.Trim('"');

        if (!string.IsNullOrWhiteSpace(contentDisposition.FileName))
            return contentDisposition.FileName.Trim('"');

        return null;
    }

    public static void OpenFile(string filePath)
    {
        var info = new System.Diagnostics.ProcessStartInfo
        {
            FileName = filePath,
            UseShellExecute = true
        };

        System.Diagnostics.Process.Start(info);
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response, string fallbackMessage)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(content))
            return fallbackMessage;

        return content;
    }
}
