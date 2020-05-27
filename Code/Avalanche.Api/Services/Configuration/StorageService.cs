using Avalanche.Api.Utility.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Configuration
{
    public class StorageService : IStorageService
    {
        readonly IFileService _fileService;

        public StorageService(IFileService fileService)
        {
            _fileService = fileService;
        }

        public async Task<T> GetJson<T>(string configurationKey)
        {
            return await _fileService.LoadAsync<T>($"/config/{configurationKey}.json");
        }

        public async Task SaveJson<T>(string configurationKey, T jsonObject)
        {
            await _fileService.SaveAsync($"/config/{configurationKey}.json", jsonObject);
        }
    }
}
