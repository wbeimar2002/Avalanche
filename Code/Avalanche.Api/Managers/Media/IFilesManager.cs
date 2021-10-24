namespace Avalanche.Api.Managers.Media
{
    public interface IFilesManager
    {
        string GetCapturePreview(string path, string procedureId, string repository);
        string GetCaptureVideo(string path, string procedureId, string repository);
    }
}
