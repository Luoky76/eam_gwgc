using Flurl.Http.Configuration;

namespace Flurl.Http
{
    internal class SSLHttpClientFactory : DefaultHttpClientFactory
    {
        public override HttpMessageHandler CreateMessageHandler()
        {
            var httpClientHandler = base.CreateMessageHandler() as HttpClientHandler;
            httpClientHandler.ServerCertificateCustomValidationCallback = (sender, certificate, chain, errors) => { return true; };
            return httpClientHandler;
        }
    }
}