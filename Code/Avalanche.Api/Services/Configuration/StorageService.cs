using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Configuration
{
    public class StorageService : IStorageService
    {
        public Task<T> GetJson<T>(string configurationKey)
        {
            throw new NotImplementedException();
        }

        public Task SaveJson(string configurationKey, string json)
        {
            throw new NotImplementedException();
        }
    }
}
