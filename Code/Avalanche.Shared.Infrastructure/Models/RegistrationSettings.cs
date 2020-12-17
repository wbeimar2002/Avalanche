using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Infrastructure.Models
{
    public class RegistrationSettings
    {
        public QuickRegistrationSettings Quick { get; set; }
        public ManualRegistrationSettings Manual{ get; set; }
    }
}
