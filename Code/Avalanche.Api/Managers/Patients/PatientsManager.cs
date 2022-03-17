using AutoMapper;
using Avalanche.Api.Services.Health;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Google.Protobuf.WellKnownTypes;

using Ism.Common.Core.Configuration.Models;
using Ism.PatientInfoEngine.V1.Protos;
using Ism.Utility.Core;
using Microsoft.AspNetCore.Http;

using System;
using System.Collections.Generic;
using System.Globalization;
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

        public PatientsManager(IPieService pieService,
            IAccessInfoFactory accessInfoFactory,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor
            )
        {
            _pieService = pieService;
            _accessInfoFactory = accessInfoFactory;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            user = HttpContextUtilities.GetUser(_httpContextAccessor.HttpContext);
            _configurationContext = _mapper.Map<UserModel, ConfigurationContext>(user);
            _configurationContext.IdnId = Guid.NewGuid().ToString();
        }

        public async Task<int> GetPatientListSource()
        {
            var getSource = await _pieService.GetPatientListSource().ConfigureAwait(false);
            return getSource.Source;
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
