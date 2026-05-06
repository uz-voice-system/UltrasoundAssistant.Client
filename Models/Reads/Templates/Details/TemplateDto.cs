using UltrasoundAssistant.DoctorClient.Models.Entity.Templates;

namespace UltrasoundAssistant.DoctorClient.Models.Reads.Templates.Details
{
    /// <summary>
    /// DTO шаблона отчёта.
    /// </summary>
    public class TemplateDto
    {
        /// <summary>
        /// Идентификатор шаблона.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Название шаблона.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Длительность приёма по умолчанию в минутах.
        /// Используется при создании записи на приём.
        /// </summary>
        public int DefaultAppointmentDurationMinutes { get; set; }

        /// <summary>
        /// Признак удаления
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Блоки шаблона
        /// </summary>
        public List<TemplateBlockDto> Blocks { get; set; } = [];

        /// <summary>
        /// Версия агрегата (для команд обновления и удаления).
        /// </summary>
        public int Version { get; set; }
    }
}
