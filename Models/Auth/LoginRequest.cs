namespace UltrasoundAssistant.DoctorClient.Models.Auth
{
    /// <summary>
    /// Запрос на аутентификацию пользователя.
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Логин пользователя.
        /// </summary>
        public string Login { get; set; } = null!;

        /// <summary>
        /// Пароль пользователя.
        /// </summary>
        public string Password { get; set; } = null!;
    }
}
