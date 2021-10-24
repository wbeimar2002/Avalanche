using Avalanche.Api.Helpers;
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

            return ProceduresHelper.GetRelativePath(procedureId, repository, path);
        }

        public string GetCaptureVideo(string path, string procedureId, string repository)
        {
            Preconditions.ThrowIfNullOrEmpty(nameof(path), path);
            Preconditions.ThrowIfNullOrEmpty(nameof(procedureId), procedureId);
            Preconditions.ThrowIfNullOrEmpty(nameof(repository), repository);

            return ProceduresHelper.GetRelativePath(procedureId, repository, path);
        }
    }
}
