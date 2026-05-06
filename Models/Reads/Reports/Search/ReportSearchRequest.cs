using UltrasoundAssistant.DoctorClient.Models.Enums;

namespace UltrasoundAssistant.DoctorClient.Models.Reads.Reports.Search;

/// <summary>
/// Фильтр поиска отчётов.
/// </summary>
public sealed class ReportSearchRequest
{
    /// <summary>
    /// Идентификатор записи на приём.
    /// </summary>
    public Guid? AppointmentId { get; set; }

    /// <summary>
    /// Идентификатор пациента.
    /// Поиск выполняется через запись на приём.
    /// </summary>
    public Guid? PatientId { get; set; }

    /// <summary>
    /// Идентификатор врача.
    /// Поиск выполняется через запись на приём.
    /// </summary>
    public Guid? DoctorId { get; set; }

    /// <summary>
    /// Идентификатор шаблона.
    /// Поиск выполняется через запись на приём.
    /// </summary>
    public Guid? TemplateId { get; set; }

    /// <summary>
    /// Статус отчёта.
    /// </summary>
    public ReportStatus? Status { get; set; }

    /// <summary>
    /// Дата создания отчёта с.
    /// </summary>
    public DateTime? CreatedFromUtc { get; set; }

    /// <summary>
    /// Дата создания отчёта по.
    /// </summary>
    public DateTime? CreatedToUtc { get; set; }

    /// <summary>
    /// Общая строка поиска.
    /// Используется для поиска по пациенту, врачу или названию шаблона.
    /// </summary>
    public string? SearchText { get; set; }

    /// <summary>
    /// Включать удалённые отчёты.
    /// </summary>
    public bool IncludeDeleted { get; set; }
}
