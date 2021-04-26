using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public class FilesService : IFilesService
    {
        public List<string> GetFiles(string folder, string filter)
        {
            return Directory.GetFiles("/app/" + folder, filter).Select(x => Path.GetFileName(x) ?? "Unknown").ToList();
        }

        public List<string> GetFolders(string folder)
        {
            return Directory.GetDirectories("/app/" + folder).Select(x => Path.GetFileName(x) ?? "Unknown").ToList();
        }
    }
}
