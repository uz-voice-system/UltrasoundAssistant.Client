namespace UltrasoundAssistant.DoctorClient.Services.AudioService;

public interface IAudioRecorderService
{
    bool IsRecording { get; }
    bool IsPaused { get; }

    Task StartAsync(CancellationToken cancellationToken = default);
    Task PauseAsync(CancellationToken cancellationToken = default);
    Task ResumeAsync(CancellationToken cancellationToken = default);
    Task<byte[]> StopAsync(CancellationToken cancellationToken = default);
}