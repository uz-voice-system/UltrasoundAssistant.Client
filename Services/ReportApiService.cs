using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using UltrasoundAssistant.DoctorClient.Models.Commands.Reports;
using UltrasoundAssistant.DoctorClient.Models.Common;
using UltrasoundAssistant.DoctorClient.Models.Reads.Reports.Details;
using UltrasoundAssistant.DoctorClient.Models.Reads.Reports.Search;
using UltrasoundAssistant.DoctorClient.Models.Statistics;

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
        var response = await _httpClient.PostAsJsonAsync("api/reports/search", filter, JsonOptions, ct);
        return await ReadQueryResultAsync<List<ReportSummaryDto>>(response, "Отчёты не найдены.");
    }

    public Task<QueryResult<List<ReportSummaryDto>>> GetAllAsync(CancellationToken ct = default)
    {
        return SearchAsync(new ReportSearchRequest(), ct);
    }

    public async Task<CommandResult> CreateAsync(CreateReportCommand command, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/reports", command, JsonOptions, ct);
        return await ReadCommandResultAsync(response, "Не удалось создать отчёт.");
    }

    public async Task<CommandResult> UpdateAsync(UpdateReportCommand command, CancellationToken ct = default)
    {
        var response = await _httpClient.PutAsJsonAsync("api/reports", command, JsonOptions, ct);
        return await ReadCommandResultAsync(response, "Не удалось обновить отчёт.");
    }

    public async Task<CommandResult> DeleteAsync(DeleteReportCommand command, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, "api/reports")
        {
            Content = JsonContent.Create(command, options: JsonOptions)
        };

        var response = await _httpClient.SendAsync(request, ct);
        return await ReadCommandResultAsync(response, "Не удалось удалить отчёт.");
    }

    public async Task<CommandResult> UploadImageAsync(
        Guid reportId,
        string filePath,
        int expectedVersion,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return CommandResult.Failure("Файл изображения не выбран.");

        if (!File.Exists(filePath))
            return CommandResult.Failure("Файл изображения не найден.");

        var bytes = await File.ReadAllBytesAsync(filePath, ct);
        var fileName = Path.GetFileName(filePath);
        var contentType = GetImageContentType(filePath);

        return await UploadImageAsync(
            reportId,
            fileName,
            bytes,
            contentType,
            expectedVersion,
            ct);
    }

    public async Task<CommandResult> UploadImageAsync(
        Guid reportId,
        string fileName,
        byte[] content,
        string? contentType,
        int expectedVersion,
        CancellationToken ct = default)
    {
        if (reportId == Guid.Empty)
            return CommandResult.Failure("Отчёт не выбран.");

        if (content.Length == 0)
            return CommandResult.Failure("Файл изображения пустой.");

        using var form = new MultipartFormDataContent();

        var fileContent = new ByteArrayContent(content);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            string.IsNullOrWhiteSpace(contentType)
                ? "application/octet-stream"
                : contentType);

        form.Add(fileContent, "File", string.IsNullOrWhiteSpace(fileName) ? "image" : fileName);

        form.Add(new StringContent(expectedVersion.ToString(CultureInfo.InvariantCulture)), "ExpectedVersion");

        var response = await _httpClient.PutAsync($"api/reports/{reportId}/image", form, ct);

        return await ReadCommandResultAsync(response, "Не удалось загрузить изображение отчёта.");
    }

    public async Task<CommandResult> DeleteImageAsync(
        Guid reportId,
        int expectedVersion,
        CancellationToken ct = default)
    {
        var command = new DeleteReportImageCommand
        {
            ReportId = reportId,
            ExpectedVersion = expectedVersion
        };

        return await DeleteImageAsync(reportId, command, ct);
    }

    public async Task<CommandResult> DeleteImageAsync(
        Guid reportId,
        DeleteReportImageCommand command,
        CancellationToken ct = default)
    {
        command.ReportId = reportId;

        var request = new HttpRequestMessage(HttpMethod.Delete, $"api/reports/{reportId}/image")
        {
            Content = JsonContent.Create(command, options: JsonOptions)
        };

        var response = await _httpClient.SendAsync(request, ct);

        return await ReadCommandResultAsync(response, "Не удалось удалить изображение отчёта.");
    }

    public async Task<QueryResult<AdminStatisticsDto>> GetAdminStatisticsAsync(
        AdminStatisticsRequest request,
        CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/reports/statistics/admin",
            request,
            JsonOptions,
            ct);

        return await ReadQueryResultAsync<AdminStatisticsDto>(
            response,
            "Не удалось получить статистику.");
    }

    public async Task<QueryResult<ReportPdfFileDto>> GetAdminStatisticsPdfAsync(
        AdminStatisticsRequest request,
        CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/reports/statistics/admin/pdf",
            request,
            JsonOptions,
            ct);

        return await ReadPdfResponseAsync(
            response,
            $"admin-statistics-{request.DateFromUtc:yyyyMMdd}-{request.DateToUtc:yyyyMMdd}.pdf",
            "Не удалось сформировать PDF статистики.",
            ct);
    }

    public async Task<QueryResult<ReportPdfFileDto>> GetPdfAsync(Guid reportId, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"api/reports/{reportId}/pdf", ct);

        return await ReadPdfResponseAsync(
            response,
            $"report-{reportId:N}.pdf",
            "Не удалось сформировать PDF отчёта.",
            ct);
    }

    private static async Task<QueryResult<ReportPdfFileDto>> ReadPdfResponseAsync(
        HttpResponseMessage response,
        string fallbackFileName,
        string fallbackErrorMessage,
        CancellationToken ct)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await ReadErrorMessageAsync(response, fallbackErrorMessage);
            return QueryResult<ReportPdfFileDto>.Failure(errorMessage);
        }

        var bytes = await response.Content.ReadAsByteArrayAsync(ct);

        var contentType = response.Content.Headers.ContentType?.ToString()
                          ?? "application/pdf";

        var fileName = GetFileName(response.Content.Headers.ContentDisposition)
                       ?? fallbackFileName;

        return QueryResult<ReportPdfFileDto>.Success(new ReportPdfFileDto
        {
            FileName = fileName,
            ContentType = contentType,
            Content = bytes
        });
    }

    private static string? GetFileName(ContentDispositionHeaderValue? contentDisposition)
    {
        if (contentDisposition is null)
            return null;

        if (!string.IsNullOrWhiteSpace(contentDisposition.FileNameStar))
            return contentDisposition.FileNameStar.Trim('"');

        if (!string.IsNullOrWhiteSpace(contentDisposition.FileName))
            return contentDisposition.FileName.Trim('"');

        return null;
    }

    private static string GetImageContentType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
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