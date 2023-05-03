using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using MassTransit;

namespace DomainManager.Requests;

public class GetCertificateInfoHandler : IConsumer<GetCertificateInfo> {
    public async Task Consume(ConsumeContext<GetCertificateInfo> context) {
        CertificateInfo? certInfo;
        try {
            certInfo = null;
            var hostname = context.Message.Hostname;

            using var client = new TcpClient(hostname, 443);
            await using var sslStream = new SslStream(client.GetStream(), false, (_, cert, chain, errors) => {
                if (cert is null) {
                    return true;
                }

                var cert2 = (X509Certificate2)cert;
                certInfo = new CertificateInfo {
                    Issuer = cert2.Issuer,
                    NotAfter = cert2.NotAfter,
                    NotBefore = cert2.NotBefore,
                    Errors = errors
                };
                return true;
            }, null);
            await sslStream.AuthenticateAsClientAsync(hostname);
            sslStream.Close();
            client.Close();
        } catch (Exception e) {
            await context.RespondAsync<ErrorResponse>(new { e.Message });
            return;
        }

        if (certInfo is null) {
            await context.RespondAsync<ErrorResponse>(new { Message = "Something went wrong" });
            return;
        }

        await context.RespondAsync(certInfo!);
    }
}