﻿using Grpc.Core;
using Grpc.Core.Interceptors;
using Ism.Security.Grpc.Models;
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
    public static class ClientHelper
    {
        public static T GetSecureClient<T>(string endpoint, string certificatePath = null, string token = null, List<Interceptor> interceptors = null, List<Func<Metadata, Metadata>> functionInterceptors = null)
        {
            string content = string.Empty;
            SslCredentials sslCredentials = null;

            var asyncAuthInterceptor = new AsyncAuthInterceptor(SetBearerToken(token));

            if (sslCredentials == null && !string.IsNullOrEmpty(certificatePath))
            {
                content = File.ReadAllText(certificatePath);
                sslCredentials = new SslCredentials(content);
            }

            var channelCredentials = ChannelCredentials.Create(sslCredentials, CallCredentials.FromInterceptor(asyncAuthInterceptor));
            var channel = new Channel(endpoint, channelCredentials);

            if (functionInterceptors != null)
            {
                foreach (var interceptor in functionInterceptors)
                {
                    channel.Intercept(interceptor);
                }
            }

            if (interceptors != null)
            {
                foreach (var interceptor in interceptors)
                {
                    channel.Intercept(interceptor);
                }
            }

            return (T)Activator.CreateInstance(typeof(T), channel);
        }

        public static T GetInsecureClient<T>(string endpoint, string token = null, List<Interceptor> interceptors = null, List<Func<Metadata, Metadata>> functionInterceptors = null)
        {
            var asyncAuthInterceptor = new AsyncAuthInterceptor(SetBearerToken(token));

            var channel = new Channel(endpoint, ChannelCredentials.Insecure);

            if (functionInterceptors != null)
            {
                foreach (var interceptor in functionInterceptors)
                {
                    channel.Intercept(interceptor);
                }
            }

            if (interceptors != null)
            {
                foreach (var interceptor in interceptors)
                {
                    channel.Intercept(interceptor);
                }
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
    }
}
