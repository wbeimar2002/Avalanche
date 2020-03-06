using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Avalanche.Host.Service.Clients;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Core.Internal;
using Grpc.Core.Utils;
using Ism.Security.Grpc.Helpers;
using Ism.Security.Grpc.Interceptors;

namespace GrpcClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var endpoint = "10.0.75.1:9012";
            var token = "Bearer SampleToken";

            List<Interceptor> interceptors = new List<Interceptor>();
            
            var client = ClientHelper.GetClient<AuthorizationServiceProto.AuthorizationServiceProtoClient>(endpoint, token, interceptors);

            var reply = await client.AuthenticateUserAsync(
                new Avalanche.Host.Service.Clients.ApplicationUser()
                {
                    Password = "DockerHost",
                    Username = "DockerHost"
                },
                new Metadata()
                {
                    new Metadata.Entry("Sample", Guid.NewGuid().ToString())
                });

            Console.WriteLine("Result: " + reply.Success);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
