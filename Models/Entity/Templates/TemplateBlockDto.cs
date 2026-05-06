namespace UltrasoundAssistant.DoctorClient.Models.Entity.Templates
{
    /// <summary>
    /// Блок шаблона отчёта
    /// </summary>
    public class TemplateBlockDto
    {
        /// <summary>
        /// Идентификатор блока
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Название блока
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Порядок отображения блока
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Фразы для определения блока
        /// </summary>
        public List<string> Phrases { get; set; } = [];

        /// <summary>
        /// Поле по умолчанию для значения после названия блока
        /// </summary>
        public string? DefaultFieldName { get; set; }

        /// <summary>
        /// Поля блока
        /// </summary>
        public List<TemplateFieldDto> Fields { get; set; } = [];
    }
}
