using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Services.Health;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Infrastructure.Configuration;
using Ism.Library.V1.Protos;
using Ism.Utility.Core;

namespace Avalanche.Api.Managers.Procedures
{
    public class ProceduresManager : IProceduresManager
    {
        private readonly ILibraryService _libraryService;
        private readonly IMapper _mapper;

        private readonly SetupConfiguration _setupConfiguration;

        public const int MinPageSize = 25;
        public const int MaxPageSize = 100;

        public ProceduresManager(
            ILibraryService libraryService,
            IMapper mapper,
            SetupConfiguration setupConfiguration)
        {
            _libraryService = libraryService;
            _mapper = mapper;
            _setupConfiguration = setupConfiguration;
        }

        public async Task UpdateProcedure(ProcedureViewModel procedureViewModel)
        {
            Preconditions.ThrowIfNull(nameof(procedureViewModel), procedureViewModel);
            Preconditions.ThrowIfNull(nameof(procedureViewModel.LibraryId), procedureViewModel.LibraryId);
            Preconditions.ThrowIfNull(nameof(procedureViewModel.Patient.MRN), procedureViewModel.Patient.MRN);
            Preconditions.ThrowIfNull(nameof(procedureViewModel.Patient.LastName), procedureViewModel.Patient.LastName);

            ValidateDynamicConditions(procedureViewModel);

            var procedure = _mapper.Map<ProcedureViewModel, ProcedureMessage>(procedureViewModel);

            await _libraryService.UpdateProcedure(new UpdateProcedureRequest
            {
                Procedure = procedure
            }).ConfigureAwait(false);
        }

        private void ValidateDynamicConditions(ProcedureViewModel procedure)
        {
            foreach (var item in _setupConfiguration.PatientInfo.Where(f => f.Required))
            {
                switch (item.Id)
                {
                    case "firstName":
                        Preconditions.ThrowIfNull(nameof(procedure.Patient.FirstName), procedure.Patient.FirstName);
                        break;
                    case "sex":
                        Preconditions.ThrowIfNull(nameof(procedure.Patient.Sex), procedure.Patient.Sex);
                        break;
                    case "dateOfBirth":
                        Preconditions.ThrowIfNull(nameof(procedure.Patient.DateOfBirth), procedure.Patient.DateOfBirth);
                        break;
                    case "physician":
                        Preconditions.ThrowIfNull(nameof(procedure.Physician), procedure.Physician);
                        break;
                    case "department":
                        Preconditions.ThrowIfNull(nameof(procedure.Department), procedure.Department);
                        break;
                    case "procedureType":
                        Preconditions.ThrowIfNull(nameof(procedure.ProcedureType), procedure.ProcedureType);
                        break;
                    case "accessionNumber":
                        Preconditions.ThrowIfNull(nameof(procedure.Accession), procedure.Accession);
                        break;
                    case "scopeSerialNumber":
                        Preconditions.ThrowIfNull(nameof(procedure.ScopeSerialNumber), procedure.ScopeSerialNumber);
                        break;
                    case "diagnosis":
                        Preconditions.ThrowIfNull(nameof(procedure.Diagnosis), procedure.Diagnosis);
                        break;
                    case "clinicalNotes":
                        Preconditions.ThrowIfNull(nameof(procedure.ClinicalNotes), procedure.ClinicalNotes);
                        break;
                    //TODO: This is not comming from UI for Update
                    //case "procedureId":
                    //    Preconditions.ThrowIfNull(nameof(procedure.ProcedureId), procedure.ProcedureId);
                    //    break;
                }
            }
        }

        public async Task<ProceduresContainerViewModel> Search(ProcedureSearchFilterViewModel filter)
        {
            Preconditions.ThrowIfNull(nameof(filter), filter);
            Preconditions.ThrowIfTrue<ArgumentException>($"{nameof(filter.Page)} must be a positive integer greater than 0", filter.Page < 0);
            Preconditions.ThrowIfTrue<ArgumentException>($"{nameof(filter.Limit)} cannot be lower than {MinPageSize}", filter.Limit < MinPageSize);
            Preconditions.ThrowIfTrue<ArgumentException>($"{nameof(filter.Limit)} cannot be larger than {MaxPageSize}", filter.Limit > MaxPageSize);

            var libraryFilter = _mapper.Map<ProcedureSearchFilterViewModel, GetFinishedProceduresRequest>(filter);
            var response = await _libraryService.GetFinishedProcedures(libraryFilter).ConfigureAwait(false);

            return new ProceduresContainerViewModel()
            {
                TotalCount = response.TotalCount,
                Procedures = _mapper.Map<IList<ProcedureMessage>, IList<ProcedureViewModel>>(response.Procedures)
            };
        }

        public async Task<ProceduresContainerViewModel> SearchByPatient(string patientId)
        {
            Preconditions.ThrowIfNull(nameof(patientId), patientId);

            var response = await _libraryService.GetFinishedProceduresByPatient(new GetFinishedProceduresRequestByPatient()
            {
                PatientId = patientId
            }).ConfigureAwait(false);

            return new ProceduresContainerViewModel()
            {
                TotalCount = response.TotalCount,
                Procedures = _mapper.Map<IList<ProcedureMessage>, IList<ProcedureViewModel>>(response.Procedures)
            };
        }

        public async Task<ProcedureViewModel> GetProcedureDetails(ProcedureIdViewModel procedureIdViewModel)
        {
            Preconditions.ThrowIfNull(nameof(procedureIdViewModel), procedureIdViewModel);
            Preconditions.ThrowIfNullOrEmptyOrWhiteSpace(nameof(procedureIdViewModel.Id), procedureIdViewModel.Id);
            Preconditions.ThrowIfNullOrEmptyOrWhiteSpace(nameof(procedureIdViewModel.RepositoryName), procedureIdViewModel.RepositoryName);

            var response = await _libraryService.GetFinishedProcedure(new GetFinishedProcedureRequest()
            {
                LibraryId = procedureIdViewModel.Id,
                RepositoryName = procedureIdViewModel.RepositoryName
            }).ConfigureAwait(false);

            return _mapper.Map<ProcedureMessage, ProcedureViewModel>(response.Procedure);
        }

        public async Task DownloadProcedure(ProcedureDownloadRequestViewModel procedureDownloadRequestModel)
        {
            Preconditions.ThrowIfNull(nameof(procedureDownloadRequestModel), procedureDownloadRequestModel);
            Preconditions.ThrowIfNull(nameof(procedureDownloadRequestModel.ProcedureId), procedureDownloadRequestModel.ProcedureId);
            Preconditions.ThrowIfNull(nameof(procedureDownloadRequestModel.ContentItemIds), procedureDownloadRequestModel.ContentItemIds);
            Preconditions.ThrowIfTrue<ArgumentException>($"{nameof(procedureDownloadRequestModel.ContentItemIds.Count)} cannot be empty", procedureDownloadRequestModel.ContentItemIds.Count == 0);
            var procedureDownloadRequest = _mapper.Map<ProcedureDownloadRequestViewModel, ProcedureDownloadRequest>(procedureDownloadRequestModel);
            await _libraryService.DownloadProcedure(procedureDownloadRequest).ConfigureAwait(false);
        }

        public async Task ShareProcedure(string repository, string id, List<string> userNames)
        {
            var request = new ShareProcedureRequest()
            {
                ProcedureId = new ProcedureIdMessage()
                {
                    Id = id,
                    RepositoryName = repository
                }
            };

            userNames.ForEach(x => request.UserNames.Add(x));

            await _libraryService.ShareProcedure(request).ConfigureAwait(false);
        }
    }
}
