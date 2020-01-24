using Avalanche.Host.Service.Services.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Avalanche.Host.Service.Services.Security
{
    public class SecurityService : ISecurityService
    {

        #region private fields

        readonly IAppLoggerService _logger;

        #endregion


        #region external pointers

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, out IntPtr phToken);

        #endregion


        #region ctor
        public SecurityService(IAppLoggerService logger)
        {
            _logger = logger;
        }

        #endregion


        #region IAuthorizationService implementation

        public bool AuthenticateUser(string username, string password)
        {
            return LogonUser(username, ".", password, 2, 0, out _);
        }

        #endregion
    }
}
