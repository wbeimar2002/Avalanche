using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Models
{
    public class Department
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public List<ProcedureType> ProcedureTypes { get; set; }
    }
}
