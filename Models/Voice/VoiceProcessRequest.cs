namespace UltrasoundAssistant.DoctorClient.Models.Voice;

public class VoiceProcessRequest
{
    public Guid ReportId { get; set; }
    public Guid TemplateId { get; set; }

    /// <summary>
    /// Клиент присылает уже WAV как base64
    /// </summary>
    public string AudioBase64 { get; set; } = string.Empty;

    /// <summary>
    /// Например "ru" или "auto"
    /// </summary>
    public string Language { get; set; } = "ru";

    /// <summary>
    /// Необязательно, но удобно для логов
    /// </summary>
    public string FileName { get; set; } = "audio.wav";
}
