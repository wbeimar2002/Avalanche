using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Avalanche.Shared.Infrastructure.Services.Settings
{
    public interface IConfigurationService
    {
        T GetValue<T>(string key);
        string GetEnvironmentVariable(string variableName);
    }
}
