using Flurl.Http.Configuration;

namespace Flurl.Http
{
    internal class SSLHttpClientFactory : DefaultHttpClientFactory
    {
        public override HttpMessageHandler CreateMessageHandler()
        {
            var httpClientHandler = base.CreateMessageHandler() as HttpClientHandler;
            httpClientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            return httpClientHandler;
        }
    }
}