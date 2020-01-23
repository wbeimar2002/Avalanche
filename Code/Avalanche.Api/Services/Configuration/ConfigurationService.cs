using Avalanche.Shared.Infrastructure.Services.Configuration;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Configuration
{
    public class ConfigurationService : IConfigurationService
    {
        readonly IConfiguration _configuration;

        public ConfigurationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public T GetValue<T>(string key)
        {
            return _configuration.GetValue<T>(key);
        }

        public async Task<TResponse> LoadAsync<TResponse>(string fileName)
        {
            try
            {
                return await Task.Run(async () =>
                {
                    if (await FileExist(fileName, null))
                    {
                        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                        var filePath = Path.Combine(documentsPath, fileName);

                        string result = System.IO.File.ReadAllText(filePath);
                        if (result.Equals("{}") || string.IsNullOrEmpty(result.Trim()) || result.Equals("\"\""))
                        {
                            return default(TResponse);
                        }
                        else
                        {
                            TResponse serializedResponse = JsonConvert.DeserializeObject<TResponse>(result);
                            return serializedResponse;
                        }
                    }
                    else
                        return default(TResponse);
                });
            }
            catch (Exception ex)
            {
                return default(TResponse);
            }
        }

        private async Task<bool> FileExist(string fileName, string contentFolder)
        {
            return await Task.Run<bool>(() =>
            {
                var folderCourse = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

                if (contentFolder != null)
                    folderCourse = Path.Combine(folderCourse, contentFolder);

                var filename = Path.Combine(folderCourse, fileName);
                return File.Exists(filename);
            });
        }
    }
}