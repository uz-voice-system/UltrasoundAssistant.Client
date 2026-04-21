using System.ComponentModel;

namespace UltrasoundAssistant.DoctorClient.Models.Enums;

/// <summary>
/// Статус отчёта ультразвукового исследования.
/// </summary>
public enum ReportStatus
{
    /// <summary>
    /// Черновик отчёта.
    /// </summary>
    [Description("Черновик")]
    Draft,

    /// <summary>
    /// Отчёт находится в процессе заполнения.
    /// </summary>
    [Description("В процессе")]
    InProgress,

    /// <summary>
    /// Отчёт завершён и сохранён.
    /// </summary>
    [Description("Завершён")]
    Completed,

    /// <summary>
    /// Отчёт перемещён в архив.
    /// </summary>
    [Description("Архивирован")]
    Archived
}
