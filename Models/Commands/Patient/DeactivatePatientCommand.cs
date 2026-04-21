namespace UltrasoundAssistant.DoctorClient.Models.Commands.Patient;

public sealed class DeactivatePatientCommand
{
    public Guid CommandId { get; set; }
    public Guid PatientId { get; set; }
    public int ExpectedVersion { get; set; }
    public string? Reason { get; set; }
}