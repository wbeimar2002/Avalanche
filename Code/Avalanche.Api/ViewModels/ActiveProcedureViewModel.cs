using System;
using System.Collections.Generic;

namespace Avalanche.Api.ViewModels
{
    public class ActiveProcedureViewModel : ProcedureDetailsViewModel
    {
        public bool RequiresUserConfirmation { get; set; }
        public int? RecorderState { get; internal set; }

        public ActiveProcedureViewModel()
        { }

        public ActiveProcedureViewModel(
            PatientViewModel patient,
            string libraryId,
            string repositoryId,
            string procedureRelativePath,
            DateTimeOffset procedureStartTimeUtc,
            string procedureTimezoneId,
            List<ProcedureImageViewModel> images,
            List<ProcedureVideoViewModel> videos,
            List<ProcedureVideoViewModel> backgroundVideos,
            bool requiresUserConfirmation)
            : base(patient, libraryId, repositoryId, procedureRelativePath, procedureStartTimeUtc, procedureTimezoneId, images, videos, backgroundVideos)
        {
            RequiresUserConfirmation = requiresUserConfirmation;
        }
    }
}
