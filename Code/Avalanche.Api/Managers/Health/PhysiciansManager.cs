using AutoFixture;
using AutoMapper;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Maintenance;
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
        readonly IMapper _mapper;

        public PhysiciansManager(IStorageService storageService, IMapper mapper)
        {
            _storageService = storageService;
            _mapper = mapper;
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
