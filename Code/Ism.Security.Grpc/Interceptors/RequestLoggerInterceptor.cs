﻿using Grpc.Core;
using Grpc.Core.Interceptors;
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
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var sw = Stopwatch.StartNew();

            var response = await base.UnaryServerHandler(request, context, continuation);

            sw.Stop();

            /* string MessageTemplate = "{RequestMethod} responded {StatusCode} in {Elapsed:0.0000} ms";
             * Use Illoger
             * Log.Logger.Information(MessageTemplate,
              context.Method,
              context.Status.StatusCode,
              sw.Elapsed.TotalMilliseconds);*/

            return response;
        }
    }
}