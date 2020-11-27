﻿using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Health
{
    public interface IPhysiciansManager
    {
        Task<List<Physician>> GetAllPhysicians();
        Task<List<Source>> GetPresetsByPhysician(string id, PresetTypes presetType);
        Task<PhysiciansViewModel> GetTemporaryPhysiciansSource(Avalanche.Shared.Domain.Models.User user);
    }
}
