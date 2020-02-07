using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.ViewModels
{
    public class PatientListViewModel
    {
        public int PageIndex { get; set; }
        public List<Patient> Patients { get; set; }
    }
}
