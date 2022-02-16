using AutoMapper;
using Avalanche.Api.Managers.Media;
using Avalanche.Api.Managers.Procedures;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Services.Security;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Configuration;
using Google.Protobuf.WellKnownTypes;

using Ism.Common.Core.Configuration.Models;
using Ism.PatientInfoEngine.V1.Protos;
using Ism.SystemState.Client;
using Ism.Utility.Core;
using Microsoft.AspNetCore.Http;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Patients
{
    public class PatientsManager : IPatientsManager
    {
        private readonly IPieService _pieService;
        private readonly IAccessInfoFactory _accessInfoFactory;
        private readonly IMapper _mapper;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserModel user;
        private readonly ConfigurationContext _configurationContext;
        private readonly SetupConfiguration _setupConfiguration;

        public PatientsManager(IPieService pieService,
            IAccessInfoFactory accessInfoFactory,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            SetupConfiguration setupConfiguration
            )
        {
            _pieService = pieService;
            _accessInfoFactory = accessInfoFactory;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _setupConfiguration = setupConfiguration;
            user = HttpContextUtilities.GetUser(_httpContextAccessor.HttpContext);
            _configurationContext = _mapper.Map<UserModel, ConfigurationContext>(user);
            _configurationContext.IdnId = Guid.NewGuid().ToString();
        }

        private void ValidateDynamicConditions(PatientViewModel patient)
        {
            foreach (var item in _setupConfiguration.PatientInfo.Where(f => f.Required))
            {
                switch (item.Id)
                {
                    case "firstName":
                        Preconditions.ThrowIfNull(nameof(patient.FirstName), patient.FirstName);
                        break;
                    case "sex":
                        Preconditions.ThrowIfNull(nameof(patient.Sex), patient.Sex);
                        break;
                    case "dateOfBirth":
                        Preconditions.ThrowIfNull(nameof(patient.DateOfBirth), patient.DateOfBirth);
                        break;

                    case "physician":
                        Preconditions.ThrowIfNull(nameof(patient.Physician), patient.Physician);
                        break;
                    case "department":
                        Preconditions.ThrowIfNull(nameof(patient.Department), patient.Department);
                        break;
                    case "procedureType":
                        Preconditions.ThrowIfNull(nameof(patient.ProcedureType), patient.ProcedureType);
                        break;
                        //case "accessionNumber": TODO: Pending send the value from Register and Update
                        //    Preconditions.ThrowIfNull(nameof(patient.Accession), patient.Accession);
                        //    break;
                }
            }
        }

        public async Task UpdatePatient(PatientViewModel existingPatient)
        {
            Preconditions.ThrowIfNull(nameof(existingPatient), existingPatient);
            Preconditions.ThrowIfNull(nameof(existingPatient.Id), existingPatient.Id);
            Preconditions.ThrowIfNull(nameof(existingPatient.MRN), existingPatient.MRN);
            Preconditions.ThrowIfNull(nameof(existingPatient.LastName), existingPatient.LastName);

            ValidateDynamicConditions(existingPatient);

            if (existingPatient.Physician == null || existingPatient.Physician.Id == 0)
            {
                existingPatient.Physician = new PhysicianModel()
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                };
            }

            var accessInfo = _accessInfoFactory.GenerateAccessInfo();

            var request = _mapper.Map<PatientViewModel, UpdatePatientRecordRequest>(existingPatient);
            request.AccessInfo = _mapper.Map<AccessInfoMessage>(accessInfo);

            await _pieService.UpdatePatient(request).ConfigureAwait(false);
        }

        public async Task DeletePatient(ulong id)
        {
            Preconditions.ThrowIfNull(nameof(id), id);

            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            var accessInfoMessage = _mapper.Map<AccessInfoMessage>(accessInfo);

            await _pieService.DeletePatient(new DeletePatientRecordRequest()
            {
                AccessInfo = accessInfoMessage,
                PatientRecordId = id
            }).ConfigureAwait(false);
        }

        public async Task<IList<PatientViewModel>> Search(PatientKeywordSearchFilterViewModel filter)
        {
            Preconditions.ThrowIfNull(nameof(filter), filter);
            Preconditions.ThrowIfNull(nameof(filter.Term), filter.Term);

            var search = new PatientSearchFieldsMessage();
            search.Keyword = filter.Term;

            // TODO - get valid culture (either system configuration or passed in via caller)
            var cultureName = CultureInfo.CurrentCulture.Name;
            cultureName = string.IsNullOrEmpty(cultureName) ? "en-US" : cultureName;

            //TODO: This is the final implementation?
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();

            var request = _mapper.Map<SearchRequest>(filter);
            request.SearchCultureName = cultureName;
            request.AccessInfo = _mapper.Map<AccessInfoMessage>(accessInfo);

            var queryResult = await _pieService.Search(request).ConfigureAwait(false);

            return _mapper.Map<IList<PatientRecordMessage>, IList<PatientViewModel>>(queryResult.UpdatedPatList);
        }

        public async Task<IList<PatientViewModel>> Search(PatientDetailsSearchFilterViewModel filter)
        {
            Preconditions.ThrowIfNull(nameof(filter), filter);

            var search = new PatientSearchFieldsMessage()
            {
                RoomName = filter.RoomName,
                LastName = filter.LastName,
                MRN = filter.MRN,
                MinDate = filter.MinDate?.ToTimestamp(),
                MaxDate = filter.MaxDate?.ToTimestamp(),
                Accession = filter.AccessionNumber,
                Keyword = null,
                Department = filter.Department,
                ProcedureId = filter.ProcedureId,
            };

            //TODO: This is the final implementation?
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();

            // TODO - get valid culture (either system configuration or passed in via caller)
            var cultureName = CultureInfo.CurrentCulture.Name;
            cultureName = string.IsNullOrEmpty(cultureName) ? "en-US" : cultureName;

            var request = _mapper.Map<SearchRequest>(filter);
            request.SearchCultureName = cultureName;
            request.AccessInfo = _mapper.Map<AccessInfoMessage>(accessInfo);

            var queryResult = await _pieService.Search(request).ConfigureAwait(false);

            return _mapper.Map<IList<PatientRecordMessage>, IList<PatientViewModel>>(queryResult.UpdatedPatList);
        }
    }
}
