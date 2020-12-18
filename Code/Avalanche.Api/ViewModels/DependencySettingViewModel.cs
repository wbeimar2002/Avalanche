﻿using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Infrastructure.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Api.ViewModels
{
    public class DependencySettingViewModel
    {
        public string Value { get; set; }
        public string JsonKey { get; set; }
    }
}