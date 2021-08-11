using System.IO;
using System.Text;
using Ism.Utility.Core.Environment;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Avalanche.Api.Helpers
{
    public static class ConfigurationHelper
    {
        public static IConfigurationSection GetCustomFeatureManagementSection()
        {
            EnvironmentUtilities.TryGetProcessThenMachineEnvironmentVariable("IsVSS", out var isVSS);

            var appSettings = JsonConvert.SerializeObject(new
            {
                FeatureManagement = new { IsVSS = isVSS ?? "false" }
            });

            var builder = new ConfigurationBuilder();

            builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(appSettings)));

            var configuration = builder.Build();

            return configuration.GetSection("FeatureManagement");
        }
    }
}
