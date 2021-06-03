using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.ViewModels
{
    public class ProcedureViewModel
    {
        public string Id { get; set; }
        public PatientModel Patient { get; set; }
        public PhysicianModel Physician { get; set; }
        public List<string> Thumbnails { get; set; }
        public string Notes { get; set; }
        public DateTime Date { get; set; }
    }
}
