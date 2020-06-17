using Ism.IsmLogCommon.Core;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Utilities
{
    public class AccessInfoFactory : IAccessInfoFactory
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccessInfoFactory(IHttpContextAccessor httpContextAccessor)
            => _httpContextAccessor = httpContextAccessor;

        public AccessInfo GenerateAccessInfo(string details = null)
        {
            string ip = GetRemoteIp();

            // TODO - this should be pulling username off of the current identity and throwing if null/empty, but I'm not sure we're ready for that yet?
            string userName = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? string.Empty;
            return new AccessInfo(ip, userName, "AvalancheApi", Environment.MachineName, details ?? string.Empty);
        }

        private string GetRemoteIp() =>
            HttpContextUtilities.GetRequestIP(_httpContextAccessor.HttpContext);
    }
}
