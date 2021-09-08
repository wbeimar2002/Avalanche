using System;
using Avalanche.Shared.Domain.Enumerations;

namespace Avalanche.Shared.Domain.Models
{
    public class PatientModel
    {
        public string? MRN { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Sexes Sex { get; set; }
    }
}
