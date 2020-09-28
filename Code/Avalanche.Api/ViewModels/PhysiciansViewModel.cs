using Avalanche.Shared.Domain.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.ViewModels
{
    public class PhysiciansViewModel
    {
        public bool DepartmentsSupported { get; set; }

        public List<ProcedureType> ProcedureTypes { get; set; }

        public List<Physician> Physicians { get; set; }
    }
}
