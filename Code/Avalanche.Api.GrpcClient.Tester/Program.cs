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

namespace Avalanche.Api.GrpcClient.Tester
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var endpoint = "10.0.75.1:9012";
            var token = "Bearer SampleToken";
            var certificatePath = @"C:\Olympus\certificates\grpc_serverl5.crt";
            
            //Interceptors are optional in te client in you send the metadata in the call
            //or you can use and interceptor for sending metadata in every call
            List<Interceptor> interceptors = new List<Interceptor>();
            List<Func<Metadata, Metadata>> functionInterceptors = new List<Func<Metadata, Metadata>>();

            var client = ClientHelper.GetSecureClient<AuthorizationServiceProto.AuthorizationServiceProtoClient>(endpoint, certificatePath, token, interceptors, functionInterceptors);

            var certificate = new X509Certificate2(certificatePath);

            Metadata metadata = new Metadata();
            metadata.Add(new Metadata.Entry("CertificateThumbprint", certificate.Thumbprint));
            metadata.Add(new Metadata.Entry("CertificateSubjectName", certificate.SubjectName.Name));

            var reply = await client.AuthenticateUserAsync(
                new Avalanche.Host.Service.Clients.ApplicationUser()
                {
                    Password = "localdevtesting",
                    Username = "MPtestAcct1"
                },
                metadata);

            Console.WriteLine("Result: " + reply.Success);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
