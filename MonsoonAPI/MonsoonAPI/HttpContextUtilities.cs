using IsmLogCommon;
using IsmUtility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using MonsoonAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MonsoonAPI {
    public class HttpContextUtilities {
        readonly HttpContext _context;

        private const string IsmUserInfoHeader = "X-ISM-USER-INFO-HEADER";

        private HttpContextUtilities(HttpContext context) => _context = context;

        public static HttpContextUtilities With(HttpContext context) => new HttpContextUtilities(context);

        public string GetRequestIP(bool tryUseXForwardHeader = true) {
            string ip = null;

            if (tryUseXForwardHeader) ip = Split(GetHeaderValue("X-Forwarded-For")).FirstOrDefault();

            // RemoteIpAddress is always null in DNX RC1 Update1 (bug).
            if (string.IsNullOrWhiteSpace(ip) && _context?.Connection?.RemoteIpAddress != null)
                ip = _context.Connection.RemoteIpAddress.ToString();

            if (string.IsNullOrWhiteSpace(ip)) ip = GetHeaderValue("REMOTE_ADDR");

            return ip;
        }

#warning this is not a safe way to do this
        // TODO - we should look at asp.net core auth/identity framework.  session/identity info should be handled by monsoonapi and passed up to php as needed, not passed in from php and blindly trusted.
        public LoggedInUserInfo GetLoggedInUserInfo()
        {
            string json = GetHeaderValue(IsmUserInfoHeader);
            if (false == string.IsNullOrEmpty(json))
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<LoggedInUserInfo>(json);
            }

            return null;
        }

        public static AccessInfo GetAccessInfoFromHttpContext(HttpContext context, string fallbackUsername = null, [CallerMemberName] string details = null) {
            var userInfo = With(context)?.GetLoggedInUserInfo();

            var userName = userInfo?.Username;
            if (string.IsNullOrWhiteSpace(userName))
                userName = fallbackUsername;
            if (string.IsNullOrEmpty(userName))
                userName = Environment.UserName;

            return new AccessInfo(userInfo?.SourceIp ?? Communications.GetFirstLocalAdapterIPv4Address(Communications.DefaultNetworkInterfaceName), userName, "MonsoonWeb", Environment.MachineName, details);
        }

        string GetHeaderValue(string headerName) {
            string returnValue = null;

            if (_context?.Request?.Headers?.TryGetValue(headerName, out StringValues values) ?? false) {
                string rawValues = values.ToString();

                if (!string.IsNullOrEmpty(rawValues))
                    returnValue = values.ToString();
            }

            return returnValue;
        }

        List<string> Split(string header, bool nullOrWhitespaceInputReturnsNull = false) {
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
