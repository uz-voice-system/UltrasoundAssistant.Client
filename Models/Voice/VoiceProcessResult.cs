namespace UltrasoundAssistant.DoctorClient.Models.Voice;

public class VoiceProcessResult
{
    public bool Matched { get; set; }

    public List<MatchedFieldResult> Fields { get; set; } = [];

    public List<string> UnmatchedParts { get; set; } = [];

    public string? Error { get; set; }
}

public class MatchedFieldResult
{
    public string FieldName { get; set; } = string.Empty;

    public string Keyword { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}
