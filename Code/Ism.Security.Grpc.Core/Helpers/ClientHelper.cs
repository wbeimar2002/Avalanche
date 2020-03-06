using Grpc.Core;
using Grpc.Core.Interceptors;
using Ism.Security.Grpc.Interceptors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Ism.Security.Grpc.Helpers

{
    public class ClientHelper 
    {
        public static T GetClient<T>(string endpoint, string certificatePath = null, List<Interceptor> interceptors = null, List<Func<Metadata, Metadata>> interceptorFunctions = null, string token = null)
        {
            var asyncAuthInterceptor = new AsyncAuthInterceptor(SetBearerToken(token));

            string content = string.Empty;
            SslCredentials sslCredentials = null;

            if (!string.IsNullOrEmpty(certificatePath))
            {
                File.ReadAllText(certificatePath);
                sslCredentials = new SslCredentials(content);
            }

            ChannelCredentials channelCredentials;

            if (sslCredentials == null)
            {
                channelCredentials = ChannelCredentials.Create(ChannelCredentials.Insecure,
                    CallCredentials.FromInterceptor(asyncAuthInterceptor));
            }
            else
            {
                channelCredentials = ChannelCredentials.Create(sslCredentials,
                    CallCredentials.FromInterceptor(asyncAuthInterceptor));
            }

            var channel = new Channel(endpoint, channelCredentials);

            foreach (var interceptor in interceptorFunctions)
            {
                channel.Intercept(interceptor);
            }

            foreach (var interceptor in interceptors)
            {
                channel.Intercept(interceptor);
            }

            return (T)Activator.CreateInstance(typeof(T), channel);
        }

        private static AsyncAuthInterceptor SetBearerToken(string token)
        {
            return async (context, metadata) =>
            {
                await Task.Delay(100).ConfigureAwait(false);  //Make sure the operation is asynchronous.
                if (!string.IsNullOrEmpty(token))
                {
                    metadata.Add("authorization", $"Bearer {token}");
                }
            };
        }

        public static ClientCertificateValidatorInterceptor VerifyCertificateInterceptor(X509Certificate2 leaf)
        {
            return async (metadata1, metadata2) =>
            {
                var result = CertificateValidatorHelper.Verify(leaf);

                metadata1 = metadata1 ?? new Metadata();
                metadata1.Add(new Metadata.Entry("CertificateIsValid", result.ToString()));
                metadata1.Add(new Metadata.Entry("CertificateThumbprint", leaf.Thumbprint));

                await Task.CompletedTask;
            };
        }

        public static ClientCertificateValidatorInterceptor VerifyCertificateInterceptor(X509Certificate2 leaf, X509Certificate2 authorithy)
        {
            return async (metadata1, metadata2) =>
            {
                var result = CertificateValidatorHelper.Verify(leaf, authorithy);

                metadata1 = metadata1 ?? new Metadata();
                metadata1.Add(new Metadata.Entry("CertificateIsValid", result.ToString()));
                metadata1.Add(new Metadata.Entry("CertificateThumbprint", leaf.Thumbprint));

                await Task.CompletedTask;
            };
        }

        public static ClientCertificateValidatorInterceptor VerifyCertificateInterceptor(string subjectName)
        {
            return async (metadata1, metadata2) =>
            {
                var result = CertificateValidatorHelper.Verify(subjectName);

                metadata1 = metadata1 ?? new Metadata();
                metadata1.Add(new Metadata.Entry("CertificateIsValid", result.ToString()));
                metadata1.Add(new Metadata.Entry("CertificateSubjectName", subjectName));

                await Task.CompletedTask;
            };
        }
    }
}
