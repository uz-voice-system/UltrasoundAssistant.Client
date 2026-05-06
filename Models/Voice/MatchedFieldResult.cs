namespace UltrasoundAssistant.DoctorClient.Models.Voice;

/// <summary>
/// Результат заполнения поля отчёта
/// </summary>
public sealed class MatchedFieldResult
{
    /// <summary>
    /// Название блока
    /// </summary>
    public string BlockName { get; set; } = string.Empty;

    /// <summary>
    /// Техническое имя поля
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Найденное ключевое слово
    /// </summary>
    public string Keyword { get; set; } = string.Empty;

    /// <summary>
    /// Распознанное ключевое слово
    /// </summary>
    public string RecognizedKeyword { get; set; } = string.Empty;

    /// <summary>
    /// Исходное значение
    /// </summary>
    public string RawValue { get; set; } = string.Empty;

    /// <summary>
    /// Нормализованное значение
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Уверенность сопоставления
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Статус проверки по норме
    /// </summary>
    public NormStatus NormStatus { get; set; } = NormStatus.Unknown;

    /// <summary>
    /// Сообщение по результату проверки нормы
    /// </summary>
    public string? NormMessage { get; set; }
}
