using Grpc.Core;
using Grpc.Core.Interceptors;
using Ism.Security.Grpc.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Ism.Security.Grpc.Interceptors
{
    public delegate Task ClientCertificateValidatorInterceptor(Metadata metadata1, Metadata metadata2);

    public class CertificateValidatorInterceptor : Interceptor
    {
        public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var subjectName = context.RequestHeaders
                .FirstOrDefault(h => h.Key.Equals("CertificateSubjectName", StringComparison.OrdinalIgnoreCase))?.Value;

            var certificateInfo = CertificateValidatorHelper.Verify(subjectName);

            if (certificateInfo.IsValid)
            {
                return continuation(request, context);
            }

            context.Status = new Status(StatusCode.Unauthenticated, "Invalid certificate");
            return default(Task<TResponse>);
        }
    }
}
