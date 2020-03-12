using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Core.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ism.Security.Grpc.Interceptors
{
    //Reference: https://gsferreira.com/archive/2019/04/logging-grpc-requests-using-serilog/
    public class RequestLoggerInterceptor : Interceptor
    {
        readonly ILogger _logger;
        public RequestLoggerInterceptor(ILogger logger)
        {
            _logger = logger;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var sw = Stopwatch.StartNew();

            var response = await base.UnaryServerHandler(request, context, continuation);

            sw.Stop();

            _logger.Debug("{RequestMethod} responded {StatusCode} in {Elapsed:0.0000} ms",
            context.Method,
            context.Status.StatusCode,
            sw.Elapsed.TotalMilliseconds);

            return response;
        }
    }
}
