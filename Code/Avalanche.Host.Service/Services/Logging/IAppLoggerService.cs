using Avalanche.Host.Service.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avalanche.Host.Service.Services.Logging
{
    public interface IAppLoggerService
    {
        void Log(LogType logType, string message, Exception ex = null, params object[] parameters);
    }
}
