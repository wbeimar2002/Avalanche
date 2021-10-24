using System;
using System.IO;
using Ism.Utility.Core;

namespace Avalanche.Api.Managers.Media
{
    public class FilesManager : IFilesManager
    {
        public string GetCapturePreview(string path, string procedureId, string repository)
        {
            Preconditions.ThrowIfNullOrEmpty(nameof(path), path);
            Preconditions.ThrowIfNullOrEmpty(nameof(procedureId), procedureId);
            Preconditions.ThrowIfNullOrEmpty(nameof(repository), repository);

            return GetRelativePath(procedureId, repository, path);
        }

        public string GetCaptureVideo(string path, string procedureId, string repository)
        {
            Preconditions.ThrowIfNullOrEmpty(nameof(path), path);
            Preconditions.ThrowIfNullOrEmpty(nameof(procedureId), procedureId);
            Preconditions.ThrowIfNullOrEmpty(nameof(repository), repository);

            return GetRelativePath(procedureId, repository, path);
        }

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
            return Path.Combine(libraryRoot, translated);
        }
    }
}
