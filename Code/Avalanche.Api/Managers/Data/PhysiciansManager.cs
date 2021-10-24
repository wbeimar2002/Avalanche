using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Utilities;
using Avalanche.Shared.Domain.Models;
using Ism.Common.Core.Configuration.Models;
using Microsoft.AspNetCore.Http;

namespace Avalanche.Api.Managers.Data
{
    public class PhysiciansManager : IPhysiciansManager
    {
        private readonly UserModel _user;
        private readonly ConfigurationContext _configurationContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private readonly IStorageService _storageService;

        public PhysiciansManager(IHttpContextAccessor httpContextAccessor, IMapper mapper, IStorageService storageService)
        {
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _storageService = storageService;

            _user = HttpContextUtilities.GetUser(_httpContextAccessor.HttpContext);
            _configurationContext = _mapper.Map<UserModel, ConfigurationContext>(_user);
            _configurationContext.IdnId = Guid.NewGuid().ToString();
        }

        public async Task<List<dynamic>> GetPhysicians()
        {
            var configurationContext = _mapper.Map<UserModel, ConfigurationContext>(_user);
            return await _storageService.GetJsonDynamicList("Physicians", 1, configurationContext);
        }
    }
}
