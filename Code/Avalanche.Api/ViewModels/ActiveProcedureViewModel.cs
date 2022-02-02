using System;
using System.Collections.Generic;
using Ism.SystemState.Models.Procedure;

namespace Avalanche.Api.ViewModels
{
    public class ActiveProcedureViewModel : ProcedureDetailsViewModel
    {
        public bool RequiresUserConfirmation { get; set; }
        public int? RecorderState { get; internal set; }
        public bool IsRecording { get; set; }
        public bool IsBackgroundRecording { get; set; }
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
            RegistrationMode registrationMode,
            PatientListSource patientListSource,
            bool requiresUserConfirmation)
            : base(patient, libraryId, repositoryId, procedureRelativePath, procedureStartTimeUtc, procedureTimezoneId, images, videos, backgroundVideos, registrationMode, patientListSource)
        {
            RequiresUserConfirmation = requiresUserConfirmation;
        }
    }
}
