using System.Net.Http.Headers;

namespace UltrasoundAssistant.DoctorClient.Helpers
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly ITokenProvider _tokenProvider;

        public AuthHeaderHandler(ITokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = _tokenProvider.GetToken();

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
