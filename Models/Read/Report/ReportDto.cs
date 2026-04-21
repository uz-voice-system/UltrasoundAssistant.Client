using UltrasoundAssistant.DoctorClient.Models.Enums;

namespace UltrasoundAssistant.DoctorClient.Models.Read.Report;

/// <summary>
/// DTO отчёта ультразвукового исследования.
/// </summary>
public class ReportDto
{
    /// <summary>
    /// Идентификатор отчёта.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор отчёта для таблиц.
    /// </summary>
    public string ShortId => Id.ToString()[..8];

    /// <summary>
    /// Статус отчёта.
    /// </summary>
    public ReportStatus Status { get; set; }

    /// <summary>
    /// Идентификатор пациента.
    /// </summary>
    public Guid PatientId { get; set; }

    /// <summary>
    /// Имя пациента.
    /// </summary>
    public string? PatientName { get; set; }

    /// <summary>
    /// Идентификатор шаблона.
    /// </summary>
    public Guid TemplateId { get; set; }

    /// <summary>
    /// Название шаблона.
    /// </summary>
    public string? TemplateName { get; set; }

    /// <summary>
    /// Содержимое отчёта.
    /// </summary>
    public Dictionary<string, string> Content { get; set; } = [];

    /// <summary>
    /// Дата создания отчёта.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Дата последнего обновления отчёта.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Версия агрегата (для команд обновления полей, завершения, удаления черновика).
    /// </summary>
    public int Version { get; set; }
}