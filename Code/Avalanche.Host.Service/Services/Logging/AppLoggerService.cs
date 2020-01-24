using Avalanche.Host.Service.Enumerations;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avalanche.Host.Service.Services.Logging
{
    public class AppLoggerService : IAppLoggerService
    {
        readonly ILogger _logger;

        public AppLoggerService()
        {
            _logger = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .WriteTo.Console()
                .CreateLogger();
        }

        public void Log(LogType logType, string message, Exception ex = null, params object[] parameters)
        {
            if (logType == LogType.Debug)
            {
                _logger.Debug(ex, message, parameters);
            }

            if (logType == LogType.Information) { _logger.Information(ex, message, parameters); }
            if (logType == LogType.Warning) { _logger.Warning(ex, message, parameters); }
            if (logType == LogType.Error) { _logger.Error(ex, message, parameters); }
            if (logType == LogType.Fatal) { _logger.Fatal(ex, message, parameters); }
        }
    }
}
