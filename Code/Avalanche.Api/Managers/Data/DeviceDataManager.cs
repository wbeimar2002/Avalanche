using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Services.Security;
using Avalanche.Shared.Domain.Models.Media;
using Avalanche.Shared.Infrastructure.Configuration;
using AvidisDeviceInterface.V1.Protos;
using Microsoft.AspNetCore.Http;

namespace Avalanche.Api.Managers.Data
{
    public class DeviceDataManager : DataManager
    {
        private readonly IAvidisService _avidisService;
        private readonly IMapper _mapper;

        public DeviceDataManager(IMapper mapper,
            IDataManagementService dataManagementService,
            IStorageService storageService,
            IHttpContextAccessor httpContextAccessor,
            SetupConfiguration setupConfiguration,
            ISecurityService securityService,
            IAvidisService avidisService) : base(mapper, dataManagementService, storageService, httpContextAccessor, setupConfiguration, securityService)
        {
            _avidisService = avidisService;
            _mapper = mapper;
        }

        public override async Task<IList<AliasIndexModel>> GetGpioPins()
        {
            var response = await _avidisService.GetGpioPins();
            return _mapper.Map<IList<AliasIndexMessage>, IList<AliasIndexModel>>(response.GpioPins);
        }
    }
}
