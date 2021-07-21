using Avalanche.Shared.Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Models
{
    public class PatientModel
    {
        public ulong? Id { get; set; }
        public string? MRN { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Sexes Sex { get; set; }
    }
}
