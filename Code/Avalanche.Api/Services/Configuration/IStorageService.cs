using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Configuration
{
    public interface IStorageService
    {
        Task<T> GetJson<T>(string configurationKey);
        Task SaveJson<T>(string configurationKey, T jsonObject);
    }
}
