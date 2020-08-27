using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.ViewModels
{
    public class PatientViewModel
    {
        public ulong? Id { get; set; }
        public string MRN { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public KeyValuePairViewModel Gender { get; set; }
        public KeyValuePairViewModel ProcedureType { get; set; }
        public Physician Physician { get; set; }
        public string ScopeSerialNumber { get; set; }
        public List<Preset> Presets { get; set; }
    }
}
