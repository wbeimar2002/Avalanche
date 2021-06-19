using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Helpers
{
    public static class ProceduresHelper
    {
        private static string GetRepositoryRelativePathFromProcedureId(string procedureId)
        {
            var strYear = procedureId.Substring(0, 4);
            var strMonth = procedureId.Substring(5, 2);
            return Path.Combine(strYear, strMonth, procedureId);
        }

        public static string GetRelativePath(string libraryId, string repository, string fileName)
        {
            var libraryRoot = Environment.GetEnvironmentVariable("LibraryDataRoot");
            var relative = GetRepositoryRelativePathFromProcedureId(libraryId);

            var itemRelative = Path.Combine(repository, relative, fileName);
            var translated = itemRelative.Replace('\\', '/').TrimStart('/');
            return System.IO.Path.Combine(libraryRoot, translated);
        }
    }
}
