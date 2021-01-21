using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.ViewModels
{
    public class ActiveProcedureViewModel : ProcedureDetailsViewModel
    {
        public bool RequiresUserConfirmation { get; set; }

        public ActiveProcedureViewModel()
        { }

        public ActiveProcedureViewModel(PatientViewModel patient, string libraryId, string repositoryId, List<ProcedureImageViewModel> images, bool requiresUserConfirmation)
            : base(patient, libraryId, repositoryId, images)
        {
            RequiresUserConfirmation = requiresUserConfirmation;
        }
    }
}
