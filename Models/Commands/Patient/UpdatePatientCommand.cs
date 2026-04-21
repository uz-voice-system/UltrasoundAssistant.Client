namespace UltrasoundAssistant.DoctorClient.Models.Commands.Patient;

public sealed class UpdatePatientCommand
{
    public Guid CommandId { get; set; }
    public Guid PatientId { get; set; }
    public int ExpectedVersion { get; set; }
    public string? FullName { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Gender { get; set; }
}