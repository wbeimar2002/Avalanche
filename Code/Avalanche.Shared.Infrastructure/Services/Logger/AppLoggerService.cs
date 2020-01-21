using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Models;
using Avalanche.Shared.Infrastructure.Services.Configuration;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Services.Logger
{
    public class AppLoggerService : IAppLoggerService
    {
        readonly IConfigurationService _configuration;
        readonly ILogger _logger;

        LoggerSettings loggerSettings;

        public AppLoggerService(IConfigurationService configuration)
        {
            _configuration = configuration;

            var logFile = configuration.GetValue<string>("Logger:File");
            var logCertificateFolder = configuration.GetValue<string>("Logger:Folder");

            var logFilePath = Path.Combine(logCertificateFolder, logFile);
            Int32 logFileSizeLimit = configuration.GetValue<int>("Logger:FileSizeLimit");
            Int32 retainedFileCountLimit = configuration.GetValue<int>("Logger:RetainedFileCountLimit");

            LoadSettings();

            LogEventLevel level = LogEventLevel.Information;
#if DEBUG
            level = LogEventLevel.Debug;
#endif

            _logger = new LoggerConfiguration()
                .ReadFrom.AppSettings()
#if DEBUG
                .MinimumLevel.Debug()
#endif
                .WriteTo.File(
                    path: logFilePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: retainedFileCountLimit,
                    fileSizeLimitBytes: logFileSizeLimit,
                    restrictedToMinimumLevel: level)
                .WriteTo.Console(restrictedToMinimumLevel: level)
                .CreateLogger();
        }

        public async void LoadSettings()
        {
            loggerSettings = await _configuration.LoadAsync<LoggerSettings>("/config/config.json");

            if (loggerSettings == null)
                loggerSettings = new LoggerSettings();
        }

        public void Log(LogType logType, string message, Exception exception = null, params object[] parameters)
        {
            if (logType == LogType.Debug)
            {
                if (loggerSettings.EnableWriteDebugLogs)
                    _logger.Information(exception, message, parameters);
                else
                    _logger.Debug(exception, message, parameters);
            }
            if (logType == LogType.Information)
            {
                _logger.Information(exception, message, parameters);
            }
            if (logType == LogType.Warning)
            {
                _logger.Warning(exception, message, parameters);
            }
            if (logType == LogType.Error)
            {
                _logger.Error(exception, message, parameters);
            }
            if (logType == LogType.Fatal)
            {
                _logger.Fatal(exception, message, parameters);
            }
        }
    }
}
