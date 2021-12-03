using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Shared.Domain.Models.Media;
using Avalanche.Shared.Infrastructure.Configuration;
using Microsoft.AspNetCore.Http;

namespace Avalanche.Api.Managers.Data
{
    public class ServerDataManager : DataManager
    {
        public ServerDataManager(IMapper mapper, IDataManagementService dataManagementService, IStorageService storageService, IHttpContextAccessor httpContextAccessor, SetupConfiguration setupConfiguration) : base(mapper, dataManagementService, storageService, httpContextAccessor, setupConfiguration)
        {
        }

        public override Task<IList<AliasIndexModel>> GetGpioPins() => throw new System.InvalidOperationException();
    }
}
