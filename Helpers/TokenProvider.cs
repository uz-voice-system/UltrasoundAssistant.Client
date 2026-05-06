namespace UltrasoundAssistant.DoctorClient.Helpers
{
    public interface ITokenProvider
    {
        string? GetToken();
        void SetToken(string token);
        void ClearToken();
    }

    public class TokenProvider : ITokenProvider
    {
        private string? _token;
        public string? GetToken() => _token;
        public void SetToken(string token) => _token = token;

        public void ClearToken() => _token = null;
    }
}
