using Grpc.Core;
using Grpc.Core.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ism.Security.Grpc.Interceptors
{
    public class AuthInterceptor : Interceptor
    {
        public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var authorizationHeader = context.RequestHeaders.FirstOrDefault(h => h.Key == "authorization");

            if (authorizationHeader == null)
            {
                return continuation(request, context);
            }
            else
            { 
                var token = authorizationHeader.Value.Split(' ').Last();
                if (token == null)
                {
                    context.Status = new Status(StatusCode.Unauthenticated, "Invalid token");
                    return default(Task<TResponse>);
                }

                if (ValidateToken(token))
                {
                    PopulateAuthContext(token, context);
                    return continuation(request, context);
                }

                context.Status = new Status(StatusCode.Unauthenticated, "Invalid token");
                return default(Task<TResponse>);
            }
        }

        private bool ValidateToken(String tokenToValidate)
        {
            return true;
        }

        private void PopulateAuthContext(String token, ServerCallContext context)
        {
            //Should be this particular to each service?
        }
    }
}
