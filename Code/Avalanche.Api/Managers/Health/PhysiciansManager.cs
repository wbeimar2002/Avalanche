using AutoFixture;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using System;
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

        public PhysiciansManager(IStorageService storageService, ISettingsService settingsService)
        {
            _storageService = storageService;
            _settingsService = settingsService;
        }

        public async Task<PhysiciansViewModel> GetTemporaryPhysiciansSource()
        {
            var setupSettings = await _settingsService.GetSetupSettingsAsync();
            if (setupSettings.DepartmentsSupported)
                return await _storageService.GetJson<PhysiciansViewModel>("PhysiciansDepartmentsSupported", 1);
            else
                return await _storageService.GetJson<PhysiciansViewModel>("PhysiciansDepartmentsNotSupported", 1);
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
