using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.ViewModels
{
    public class ProcedureViewModel
    {
        public string LibraryId { get; set; }

        public PatientModel Patient { get; set; }
        public DepartmentModel Department { get; set; }
        public ProcedureTypeModel ProcedureType { get; set; }
        public PhysicianModel Physician { get; set; }
        public NoteModel Notes { get; set; }

        public DateTimeOffset ProcedureStartTimeUtc { get; set; }

        public List<ProcedureImageViewModel> Images { get; set; }
        public List<ProcedureVideoViewModel> Videos { get; set; }
    }
}
