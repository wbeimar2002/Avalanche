using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Grpc.Core.Server;

namespace Ism.Security.Grpc.Helpers
{
    public static class ServerHelper
    {
        public static Server GetServer(string _hostname, int _port,
            List<ServerServiceDefinition> serverServiceDefinitions,
            List<KeyCertificatePair> keyCertificatePairs)
        {
            List<KeyCertificatePair> certs = new List<KeyCertificatePair>();
            ServerCredentials serverCredentials;

            var server = new Server(new[] { new ChannelOption(ChannelOptions.SoReuseport, 0) });

            foreach (var service in serverServiceDefinitions)
            {
                server.Services.Add(service);
            }

            if (keyCertificatePairs == null || keyCertificatePairs.Count == 0)
            {
                server.Ports.Add(new ServerPort(_hostname, _port, ServerCredentials.Insecure));
            }
            else
            {
                foreach (var key in keyCertificatePairs)
                {
                    certs.Add(key);
                }
                
                serverCredentials = new SslServerCredentials(certs);
                server.Ports.Add(new ServerPort(_hostname, _port, serverCredentials));
            }

            return server;
        }
    }
}
