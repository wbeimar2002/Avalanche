using System.Collections.Generic;

namespace Avalanche.Api.ViewModels
{
    public class ProcedureDetailsViewModel
    {
        public PatientViewModel Patient { get; set; }
        public string LibraryId { get; set; }

        public string RepositoryId { get; set; }

        public List<ProcedureImageViewModel> Images { get; set; }

        public ProcedureDetailsViewModel()
        {
        }

        public ProcedureDetailsViewModel(PatientViewModel patient, string libraryId, string repositoryId, List<ProcedureImageViewModel> images)
        {
            Patient = patient;
            LibraryId = libraryId;
            RepositoryId = repositoryId;
            Images = images;
        }
    }
}
