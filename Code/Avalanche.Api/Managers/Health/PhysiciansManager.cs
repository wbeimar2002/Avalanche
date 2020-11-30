﻿using AutoFixture;
using AutoMapper;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Ism.Common.Core.Configuration.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Health
{
    [ExcludeFromCodeCoverage]
    public class PhysiciansManager : IPhysiciansManager
    {
        readonly IStorageService _storageService;
        readonly ISettingsService _settingsService;
        readonly IMapper _mapper;

        public PhysiciansManager(IStorageService storageService, ISettingsService settingsService, IMapper mapper)
        {
            _storageService = storageService;
            _settingsService = settingsService;
            _mapper = mapper;
        }

        public async Task<PhysiciansViewModel> GetTemporaryPhysiciansSource(Avalanche.Shared.Domain.Models.User user)
        {
            var configurationContext = _mapper.Map<Avalanche.Shared.Domain.Models.User, ConfigurationContext>(user);

            var setupSettings = await _settingsService.GetSetupSettingsAsync(configurationContext);
            if (setupSettings.DepartmentsSupported)
                return await _storageService.GetJson<PhysiciansViewModel>("PhysiciansDepartmentsSupported", 1, configurationContext);
            else
                return await _storageService.GetJson<PhysiciansViewModel>("PhysiciansDepartmentsNotSupported", 1, configurationContext);
        }

        public Task<List<Physician>> GetAllPhysicians()
        {
            var fixture = new Fixture();
            return Task.FromResult(fixture.CreateMany<Physician>(10).ToList());
        }

        public Task<List<Source>> GetPresetsByPhysician(string id, PresetTypes presetType)
        {
            var fixture = new Fixture();
            return Task.FromResult(fixture.CreateMany<Source>(10).ToList());
        }
    }
}
