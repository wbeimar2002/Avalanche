using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace MonsoonAPI.Middleware {
    public class IpGatewayMiddleware {
        const string LOCALHOST = "localhost";
        const string IP_ADDRESS = "127.0.0.1";

        readonly RequestDelegate _next;

        public IpGatewayMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext context) {
            var host = context.Request.Host.Host;

            if (!host.Equals(LOCALHOST, StringComparison.InvariantCultureIgnoreCase) && !host.Equals(IP_ADDRESS)) {
                if (!context.Request.Path.Value.StartsWith("/vip/", StringComparison.InvariantCultureIgnoreCase)) {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("Access to this resource is not allowed outside of the hosted system.");

                    return;
                }

            }

            await _next(context);
        }
    }

    public static class IpGatewayMiddlewareExtension {
        public static IApplicationBuilder UseIpGateway(this IApplicationBuilder builder) {
            return builder.UseMiddleware<IpGatewayMiddleware>();
        }
    }
}
