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
        public string Repository { get; set; }
        public DateTime ProcedureStartTimeUtc { get; set; }
        public bool IsClinical { get; set; }

        public PatientModel Patient { get; set; }
        public DepartmentModel Department { get; set; }
        public ProcedureTypeModel ProcedureType { get; set; }
        public PhysicianModel Physician { get; set; }
        public IList<NoteModel> Notes { get; set; }

        public List<ImageContentViewModel> Images { get; set; }
        public List<VideoContentViewModel> Videos { get; set; }
    }
}
