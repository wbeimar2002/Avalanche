﻿using AutoFixture;
using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Health
{
    [ExcludeFromCodeCoverage]
    public class PhysiciansManagerMock : IPhysiciansManager
    {
        public Task<List<Physician>> GetAllPhysicians()
        {
            var fixture = new Fixture();
            return Task.FromResult(fixture.CreateMany<Physician>(10).ToList());
        }

        public Task<List<Preset>> GetPresetsByPhysician(string id)
        {
            var fixture = new Fixture();
            return Task.FromResult(fixture.CreateMany<Preset>(10).ToList());
        }
    }
}
