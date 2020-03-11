using Grpc.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static Grpc.Core.Server;

namespace Ism.Security.Grpc.Helpers
{
    public static class ServerHelper
    {
        public static Server GetSecureServer(string _hostname, int _port, 
            string certificateFilePath, string certificateKeyFilePath, int chainLevels,
            List<ServerServiceDefinition> serverServiceDefinitions)
        {
            List<KeyCertificatePair> keyCertificatePairs = new List<KeyCertificatePair>()
            { new KeyCertificatePair(File.ReadAllText(certificateFilePath), File.ReadAllText(certificateKeyFilePath)) };

            var certificate = new X509Certificate2(certificateFilePath);
            var certificateInfo = CertificateValidatorHelper.Verify(certificate);

            if (!certificateInfo.IsValid || certificateInfo.Chain == null || certificateInfo.Chain.Count != chainLevels)
                throw new Exception("Please install the right chain of certificates in the server.");

            var server = new Server(new[] { new ChannelOption(ChannelOptions.SoReuseport, 0) });

            foreach (var service in serverServiceDefinitions)
            {
                server.Services.Add(service);
            }

            ServerCredentials serverCredentials = new SslServerCredentials(keyCertificatePairs);
            server.Ports.Add(new ServerPort(_hostname, _port, serverCredentials));           

            return server;
        }

        public static Server GetInsecureServer(string _hostname, int _port,
            List<ServerServiceDefinition> serverServiceDefinitions)
        {
            var server = new Server(new[] { new ChannelOption(ChannelOptions.SoReuseport, 0) });

            foreach (var service in serverServiceDefinitions)
            {
                server.Services.Add(service);
            }

            server.Ports.Add(new ServerPort(_hostname, _port, ServerCredentials.Insecure));

            return server;
        }
    }
}
