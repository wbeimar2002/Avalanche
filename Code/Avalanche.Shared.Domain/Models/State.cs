using Avalanche.Shared.Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Models
{
    public class State
    {
        public StateTypes StateType { get; set; }
        public string Value { get; set; }
    }
}
