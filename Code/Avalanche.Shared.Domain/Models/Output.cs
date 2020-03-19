using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Models
{
    public class Output
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Preview { get; set; }
        public bool IsActive { get; set; }

        List<State> States { get; set; }
    }
}
