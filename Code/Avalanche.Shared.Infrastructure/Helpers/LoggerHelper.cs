using Avalanche.Shared.Infrastructure.Enumerations;

namespace Avalanche.Shared.Infrastructure.Helpers
{
    public class LoggerHelper
    {
        private static string GetFileName(string path)
        {
            var parts = path.Split("\\");
            string fileName = parts[parts.Length - 1];
            string className = fileName.Split('.')[0];

            return className;
        }

        public static string GetLogMessage(DebugLogType debugLogType,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
        {
            var message = $"{debugLogType.ToString()} {GetFileName(sourceFilePath)}.{memberName}".Trim();
            return message;
        }
    }
}