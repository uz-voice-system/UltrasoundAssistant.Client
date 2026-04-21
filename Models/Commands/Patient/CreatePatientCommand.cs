namespace UltrasoundAssistant.DoctorClient.Models.Commands.Patient;

public sealed class CreatePatientCommand
{
    public Guid CommandId { get; set; }
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string? Gender { get; set; }
}