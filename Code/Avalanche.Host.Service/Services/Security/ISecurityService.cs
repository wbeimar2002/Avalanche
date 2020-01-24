using Avalanche.Host.Service.Services.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avalanche.Host.Service.Services.Security
{
    public interface ISecurityService
    {
        bool AuthenticateUser(string username, string password);
    }
}
