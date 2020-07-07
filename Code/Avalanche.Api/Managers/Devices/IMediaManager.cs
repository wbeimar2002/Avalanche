﻿using Avalanche.Api.ViewModels;
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
        Task<List<CommandResponse>> SendCommandAsync(CommandViewModel command);
        Task<TimeoutSettings> GetTimeoutSettingsAsync();
        Task SaveFileAsync(IFormFile file);
    }
}