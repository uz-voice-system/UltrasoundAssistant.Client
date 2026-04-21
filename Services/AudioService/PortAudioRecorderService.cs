using PortAudioSharp;
using System.Runtime.InteropServices;

namespace UltrasoundAssistant.DoctorClient.Services.AudioService;

public sealed class PortAudioRecorderService : IAudioRecorderService, IDisposable
{
    private readonly object _sync = new();

    private PortAudioSharp.Stream? _stream;
    private StreamParameters _inputParameters;
    private readonly List<float> _samples = new();

    private int _sampleRate = 16000;
    private short _channels = 1;
    private bool _initialized;

    // Держим callback в поле, чтобы GC не собрал делегат
    private PortAudioSharp.Stream.Callback? _callback;

    public bool IsRecording { get; private set; }
    public bool IsPaused { get; private set; }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            if (IsRecording)
                throw new InvalidOperationException("Запись уже запущена.");

            EnsureInitialized();

            _samples.Clear();

            var deviceIndex = PortAudio.DefaultInputDevice;
            if (deviceIndex == PortAudio.NoDevice)
                throw new InvalidOperationException("Не найдено устройство ввода по умолчанию.");

            var deviceInfo = PortAudio.GetDeviceInfo(deviceIndex);

            _inputParameters = new StreamParameters
            {
                device = deviceIndex,
                channelCount = _channels,
                sampleFormat = SampleFormat.Float32,
                suggestedLatency = deviceInfo.defaultLowInputLatency,
                hostApiSpecificStreamInfo = IntPtr.Zero
            };

            _callback = OnAudioCallback;

            _stream = new PortAudioSharp.Stream(
                inParams: _inputParameters,
                outParams: null,
                sampleRate: _sampleRate,
                framesPerBuffer: 0,
                streamFlags: StreamFlags.ClipOff,
                callback: _callback,
                userData: IntPtr.Zero
            );

            _stream.Start();

            IsRecording = true;
            IsPaused = false;
        }

        return Task.CompletedTask;
    }

    public Task PauseAsync(CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            if (!IsRecording)
                throw new InvalidOperationException("Запись не запущена.");

            IsPaused = true;
        }

        return Task.CompletedTask;
    }

    public Task ResumeAsync(CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            if (!IsRecording)
                throw new InvalidOperationException("Запись не запущена.");

            IsPaused = false;
        }

        return Task.CompletedTask;
    }

    public Task<byte[]> StopAsync(CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            if (!IsRecording)
                throw new InvalidOperationException("Запись не запущена.");

            try
            {
                _stream?.Stop();
                _stream?.Dispose();
                _stream = null;

                var wavBytes = BuildWaveFromFloatSamples(_samples, _sampleRate, _channels);

                IsRecording = false;
                IsPaused = false;

                return Task.FromResult(wavBytes);
            }
            finally
            {
                IsRecording = false;
                IsPaused = false;
            }
        }
    }

    private StreamCallbackResult OnAudioCallback(
        IntPtr input,
        IntPtr output,
        uint frameCount,
        ref StreamCallbackTimeInfo timeInfo,
        StreamCallbackFlags statusFlags,
        IntPtr userData)
    {
        lock (_sync)
        {
            if (!IsRecording || IsPaused || input == IntPtr.Zero)
                return StreamCallbackResult.Continue;

            var count = checked((int)(frameCount * (uint)_channels));
            var buffer = new float[count];

            Marshal.Copy(input, buffer, 0, count);
            _samples.AddRange(buffer);

            return StreamCallbackResult.Continue;
        }
    }

    private void EnsureInitialized()
    {
        if (_initialized)
            return;

        PortAudio.Initialize();
        _initialized = true;
    }

    private static byte[] BuildWaveFromFloatSamples(List<float> samples, int sampleRate, short channels)
    {
        // Конвертируем float [-1..1] в PCM16
        var pcm16 = new short[samples.Count];

        for (int i = 0; i < samples.Count; i++)
        {
            var s = Math.Clamp(samples[i], -1.0f, 1.0f);
            pcm16[i] = (short)Math.Round(s * short.MaxValue);
        }

        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        var bytesPerSample = 2; // PCM16
        var blockAlign = (short)(channels * bytesPerSample);
        var byteRate = sampleRate * blockAlign;
        var dataSize = pcm16.Length * bytesPerSample;

        // RIFF header
        bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
        bw.Write(36 + dataSize);
        bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

        // fmt chunk
        bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
        bw.Write(16);                  // PCM chunk size
        bw.Write((short)1);            // PCM
        bw.Write(channels);
        bw.Write(sampleRate);
        bw.Write(byteRate);
        bw.Write(blockAlign);
        bw.Write((short)16);           // bits per sample

        // data chunk
        bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
        bw.Write(dataSize);

        foreach (var sample in pcm16)
            bw.Write(sample);

        bw.Flush();
        return ms.ToArray();
    }

    public void Dispose()
    {
        lock (_sync)
        {
            try
            {
                _stream?.Dispose();
                _stream = null;
            }
            catch
            {
                // ignore
            }

            if (_initialized)
            {
                try
                {
                    PortAudio.Terminate();
                }
                catch
                {
                    // ignore
                }

                _initialized = false;
            }

            IsRecording = false;
            IsPaused = false;
        }
    }
}