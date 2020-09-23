using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Devices
{
    public interface IMediaManager
    {
        Task<IList<Device>> GetSourceStreams();
        Task SaveFileAsync(IFormFile file);
        Task<Content> GetContent(string contentType);
    }
}
