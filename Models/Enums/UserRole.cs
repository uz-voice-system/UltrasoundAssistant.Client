using System.ComponentModel;
using System.Text.Json.Serialization;

namespace UltrasoundAssistant.DoctorClient.Models.Enum;

/// <summary>
/// Роли пользователей в системе.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
    /// <summary>
    /// Врач-диагност.
    /// </summary>
    [Description("Врач")]
    Doctor,

    /// <summary>
    /// Администратор системы.
    /// </summary>
    [Description("Администратор")]
    Admin,

    /// <summary>
    /// Регистратор (ввод данных пациентов).
    /// </summary>
    [Description("Регистратор")]
    Registrar
}
