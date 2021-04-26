using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Media
{
    public interface IFilesService
    {
        List<string> GetFiles(string folder, string filter);
        List<string> GetFolders(string folder);
    }
}
