namespace UltrasoundAssistant.DoctorClient.Models.Voice;

public class VoiceProcessResult
{
    public bool Matched { get; set; }

    public List<MatchedFieldResult> Fields { get; set; } = [];

    public List<string> UnmatchedParts { get; set; } = [];

    public string? Error { get; set; }
}
