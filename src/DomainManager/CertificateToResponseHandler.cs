using System.Security.Authentication;
using System.Text.Json;

public class CertificateToResponseHandler : HttpClientHandler {
    public CertificateToResponseHandler(ILogger<CertificateToResponseHandler> logger) {
        ClientCertificates.Clear();
        ClientCertificateOptions = ClientCertificateOption.Manual;
        SslProtocols = SslProtocols.Tls12;
        CheckCertificateRevocationList = true;
        ServerCertificateCustomValidationCallback = (request, cert, chain, sslErrors) => {
            logger.LogInformation("!!!! Callback !!!");
            var content = JsonSerializer.Serialize(new {
                cert,
                chain,
                sslErrors
            });
            request.Headers.Add("SslCertificate", content);
            return false;
        };
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken) {
        return Task.FromResult(new HttpResponseMessage {
            Content = new StringContent((request.Headers.TryGetValues("SslCertificate", out var content)
                ? content.FirstOrDefault()
                : "No certificate")!)
        });
    }
}