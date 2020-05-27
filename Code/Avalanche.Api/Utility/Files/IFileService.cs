using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Avalanche.Api.Utility.Files
{
    public interface IFileService
    {
        Task SaveAsync<T>(string fileName, T content);
        Task SaveStringAsync(string fileName, string content);
        Task<T> LoadAsync<T>(string filename);
        Task<bool> ExistsRecentCache(string fileName, int cacheTime);
        Task DeleteFiles(string contentFolder = null);
    }
}
