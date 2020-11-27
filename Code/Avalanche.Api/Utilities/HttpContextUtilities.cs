using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Avalanche.Api.Utilities
{
    public static class HttpContextUtilities
    {
        public static string GetRequestIP(HttpContext context, bool tryUseXForwardHeader = true)
        {
            string ip = null;

            // prefer forwarded ip
            if (tryUseXForwardHeader)
            {
                ip = Split(GetHeaderValue(context, "X-Forwarded-For")).FirstOrDefault();
            }

            // else context remote ip
            if (string.IsNullOrWhiteSpace(ip) && context?.Connection?.RemoteIpAddress != null)
            {
                ip = context.Connection.RemoteIpAddress.ToString();
            }

            // not sure this ever happens anymore (may have been old asp.net core bug), but fallback to remote_addr header as final attempt
            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = GetHeaderValue(context, "REMOTE_ADDR");
            }
            return ip;
        }

        public static string GetHeaderValue(HttpContext context, string headerName)
        {
            string returnValue = null;

            if (context?.Request?.Headers?.TryGetValue(headerName, out StringValues values) ?? false)
            {
                string rawValues = values.ToString();

                if (!string.IsNullOrEmpty(rawValues))
                    returnValue = values.ToString();
            }

            return returnValue;
        }

        public static string GetHostAddress(HttpContext context, bool replaceLocalhostWithLoopback)
        {
            var host = context?.Request?.Host.Host;
            if (replaceLocalhostWithLoopback && string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase))
            {
                return "127.0.0.1";
            }
            return host;
        }

        private static List<string> Split(string header, bool nullOrWhitespaceInputReturnsNull = false)
        {
            if (string.IsNullOrWhiteSpace(header))
                return nullOrWhitespaceInputReturnsNull ? null : new List<string>();

            return header
                    .TrimEnd(',')
                    .Split(',')
                    .Select(s => s.Trim())
                    .ToList();
        }
    }
}
