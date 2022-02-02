using System;
using System.Collections.Generic;
using Ism.SystemState.Models.Procedure;

namespace Avalanche.Api.ViewModels
{
    public class ProcedureDetailsViewModel
    {
        public PatientViewModel Patient { get; set; }
        public string LibraryId { get; set; }

        public string RepositoryId { get; set; }
        public string ProcedureRelativePath { get; set; }
        public DateTimeOffset ProcedureStartTimeUtc { get; set; }
        public string ProcedureTimezoneId { get; set; }

        public List<ProcedureImageViewModel> Images { get; set; }
        public List<ProcedureVideoViewModel> Videos { get; set; }
        public List<ProcedureVideoViewModel> BackgroundVideos { get; set; }

        /// <summary>
        /// Indicating The registration mode: Manual or Quick
        /// </summary>
        public RegistrationMode RegistrationMode { get; set; }

        /// <summary>
        /// Indicating The Patient List Source: ERM Based Patient List or Local Patient List
        /// </summary>
        public PatientListSource PatientListSource { get; set; }

        public ProcedureDetailsViewModel()
        {
        }

        public ProcedureDetailsViewModel(
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
            PatientListSource patientListSource)
        {
            Patient = patient;
            LibraryId = libraryId;
            RepositoryId = repositoryId;
            ProcedureRelativePath = procedureRelativePath;
            ProcedureStartTimeUtc = procedureStartTimeUtc;
            ProcedureTimezoneId = procedureTimezoneId;

            Images = images;
            Videos = videos;
            BackgroundVideos = backgroundVideos;
            RegistrationMode = registrationMode;
            PatientListSource = patientListSource;
        }
    }
}
