using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Avalanche.Shared.Infrastructure.Services.Configuration
{
    public interface IConfigurationService
    {
        T GetValue<T>(string key);
        Task<TResponse> LoadAsync<TResponse>(string fileName);
    }
}
