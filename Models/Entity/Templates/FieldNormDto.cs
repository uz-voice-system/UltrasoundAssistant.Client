namespace UltrasoundAssistant.DoctorClient.Models.Entity.Templates;

/// <summary>
/// Норма значения поля
/// </summary>
public sealed class FieldNormDto
{
    /// <summary>
    /// Минимальное допустимое значение
    /// </summary>
    public decimal? Min { get; set; }

    /// <summary>
    /// Максимальное допустимое значение
    /// </summary>
    public decimal? Max { get; set; }

    /// <summary>
    /// Единица измерения нормы
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Нормальное текстовое значение
    /// </summary>
    public string? NormalText { get; set; }
}
