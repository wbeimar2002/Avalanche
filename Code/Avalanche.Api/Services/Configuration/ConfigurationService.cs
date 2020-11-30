using Avalanche.Shared.Infrastructure.Services.Settings;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Avalanche.Api.Services.Settings
{
    [ExcludeFromCodeCoverage]
    public class ConfigurationService : IConfigurationService
    {
        readonly IConfiguration _configuration;

        public ConfigurationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetEnvironmentVariable(string variableName)
        {
            return Environment.GetEnvironmentVariable(variableName);
        }

        public T GetValue<T>(string key)
        {
            return _configuration.GetValue<T>(key);
        }
    }
}
