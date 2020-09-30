using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Avalanche.Shared.Domain.Models
{
    public class Patient
    {
        public ulong Id { get; set; }
        public string MRN { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Sex { get; set; }
        public string Department { get; set; }
    }
}
