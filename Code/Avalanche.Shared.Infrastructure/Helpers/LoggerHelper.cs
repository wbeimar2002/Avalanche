using System;
using System.Runtime.CompilerServices;
using Avalanche.Shared.Infrastructure.Enumerations;
using Microsoft.Extensions.Logging;

namespace Avalanche.Shared.Infrastructure.Helpers
{
    public static class LoggerHelper
    {
        private static string GetFileName(string path)
        {
            var parts = path.Split("\\");
            var fileName = parts[^1];
            var className = fileName.Split('.')[0];

            return className;
        }

        public static string GetLogMessage(DebugLogType debugLogType,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
        {
            var message = $"{debugLogType} {GetFileName(sourceFilePath)}.{memberName}".Trim();
            return message;
        }

        private static string GetMessage(DebugLogType debugLogType, string memberName, string sourceFilePath) => $"{debugLogType} {GetFileName(sourceFilePath)}.{memberName}".Trim();

        public static void LogRequested(this ILogger logger, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "") =>
            logger.LogDebug(GetMessage(DebugLogType.Requested, memberName, sourceFilePath));

        public static void LogCompleted(this ILogger logger, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "") =>
            logger.LogDebug(GetMessage(DebugLogType.Completed, memberName, sourceFilePath));

        public static void LogException(this ILogger logger, Exception ex, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "") =>
            logger.LogError(ex, GetMessage(DebugLogType.Exception, memberName, sourceFilePath));
    }
}
