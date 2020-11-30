using Avalanche.Shared.Domain.Models;
using System.Collections.Generic;

namespace Avalanche.Api.ViewModels
{
    public class PhysiciansViewModel
    {
        public bool DepartmentsSupported { get; set; }

        public List<Physician> Physicians { get; set; }
    }
}
