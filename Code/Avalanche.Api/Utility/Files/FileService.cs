using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Avalanche.Api.Utility.Files
{
    public class FileService : IFileService
    {
        public async Task SaveAsync<T>(string fileName, T content)
        {
            await Task.Run(() =>
            {
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                var filePath = Path.Combine(documentsPath, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                string result = JsonConvert.SerializeObject(content);
                File.WriteAllText(filePath, result);
            });
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

        public async Task<bool> ExistsRecentCache(string fileName, int cacheTime)
        {
            return await Task.Run<bool>(() =>
            {
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                var filePath = Path.Combine(documentsPath, fileName);
                if (File.Exists(filePath))
                {
                    var creationTime = File.GetCreationTime(filePath);
                    if (creationTime < DateTime.Now.Add(new TimeSpan(0, cacheTime, 0, 0)))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            });
        }

        public async Task<bool> FileExist(string fileName, string contentFolder)
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

        public async Task DeleteFiles(string contentFolder = null)
        {
            await Task.Run(() =>
            {
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                var filePath = documentsPath;

                if (!string.IsNullOrEmpty(contentFolder))
                {
                    filePath = Path.Combine(documentsPath, contentFolder);
                }

                if (Directory.Exists(filePath))
                {
                    string[] files = Directory.GetFiles(filePath);
                    foreach (string file in files)
                    {
                        File.SetAttributes(file, FileAttributes.Normal);
                        File.Delete(file);
                    }
                }
            });
        }

        public async Task SaveStringAsync(string fileName, string content)
        {
            await Task.Run(() =>
            {
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                var filePath = Path.Combine(documentsPath, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                File.WriteAllText(filePath, content);
            });
        }
    }
}