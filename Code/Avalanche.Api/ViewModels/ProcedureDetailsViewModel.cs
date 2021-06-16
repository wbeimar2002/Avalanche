using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;

namespace Avalanche.Api.ViewModels
{
    public class ProcedureDetailsViewModel
    {
        public PatientModel Patient { get; set; }
        public KeyValuePairViewModel Sex { get; set; }
        public DepartmentModel Department { get; set; }
        public ProcedureTypeModel ProcedureType { get; set; }
        public PhysicianModel Physician { get; set; }
        public string LibraryId { get; set; }

        public string RepositoryId { get; set; }
        public string ProcedureRelativePath { get; set; }
        public DateTimeOffset ProcedureStartTimeUtc { get; set; }
        public string ProcedureTimezoneId { get; set; }

        public List<ProcedureImageViewModel> Images { get; set; }
        public List<ProcedureVideoViewModel> Videos { get; set; }

        public ProcedureDetailsViewModel()
        {
        }

        public ProcedureDetailsViewModel(
            PatientModel patient,
            string libraryId,
            string repositoryId,
            string procedureRelativePath,
            DateTimeOffset procedureStartTimeUtc,
            string procedureTimezoneId,
            List<ProcedureImageViewModel> images,
            List<ProcedureVideoViewModel> videos)
        {
            Patient = patient;
            LibraryId = libraryId;
            RepositoryId = repositoryId;
            ProcedureRelativePath = procedureRelativePath;
            ProcedureStartTimeUtc = procedureStartTimeUtc;
            ProcedureTimezoneId = procedureTimezoneId;

            Images = images;
            Videos = videos;
        }
    }
}
