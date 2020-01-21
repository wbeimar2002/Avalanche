using Avalanche.Shared.Infrastructure.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Services.Logger
{
    public interface IAppLoggerService
    {
        void Log(LogType logType, string message, Exception ex = null, params object[] parameters);
    }
}
