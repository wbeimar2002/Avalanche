using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Shared.Domain.Models
{
    public class Patient
    {
        public ulong Id { get; set; }
        public string MRN { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Department { get; set; }
        public string AccessionNumber { get; set; }
        public string Room { get; set; }
        public string ProcedureType { get; set; }

        // TODO - evaluate remaining properties from storage model (physicians, schedule info, adminission status, extended properties) - are they valuable / necessary for the UI?
    }
}
