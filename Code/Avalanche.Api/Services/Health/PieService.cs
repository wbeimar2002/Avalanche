using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Services.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ism.PatientInfoEngine.Client.Core;
using Ism.Security.Grpc.Helpers;
using System.Globalization;
using Avalanche.Api.Mapping.Health;
using Ism.PatientInfoEngine.Common.Core.Models;
using Ism.PatientInfoEngine.Common.Core;
using Ism.PatientInfoEngine.Common.Core.proto;
using Avalanche.Api.Utility;

namespace Avalanche.Api.Services.Health
{
    public class PieService : IPieService
    {
        readonly IConfigurationService _configurationService;
        readonly IPieMapping _pieMapping;
        readonly string _hostIpAddress;
        readonly IAccessInfoFactory _accessInfoFactory;

        public IPatientList Client { get; set; }

        public PieService(IConfigurationService configurationService, IPieMapping pieMapping, IAccessInfoFactory accessInfoFactory)
        {
            _configurationService = configurationService;
            _pieMapping = pieMapping;
            _accessInfoFactory = accessInfoFactory;

            _hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");

            var patientInfoEngineGrpcPort = _configurationService.GetEnvironmentVariable("PatientInfoEngineGrpcPort");
            var grpcCertificate = _configurationService.GetEnvironmentVariable("grpcCertificate");
            var grpcPassword = _configurationService.GetEnvironmentVariable("grpcPassword");

            var certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(grpcCertificate, grpcPassword);

            var client = ClientHelper.GetInsecureClient<PatientListService.PatientListServiceClient>($"https://{_hostIpAddress}:{patientInfoEngineGrpcPort}");
            Client = new PatientListClient(client);
        }

        public Task<List<Physician>> GetPhysiciansByPatient(string patientId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Procedure>> GetProceduresByPhysicianAndPatient(string patientId, string physicianId)
        {
            throw new NotImplementedException();
        }

        public Task<Patient> RegisterPatient(Patient newPatient)
        {
            throw new NotImplementedException();
        }

        public Task<Patient> RegisterQuickPatient()
        {
            throw new NotImplementedException();
        }

        public async Task<List<Patient>> Search(PatientKeywordSearchFilterViewModel filter)
        {
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            var search = new PatientSearchFields(null, null, null, null, null, null, filter.Term, null, null);

            // TODO - get valid culture (either system configuration or passed in via caller)
            var cultureName = CultureInfo.CurrentCulture.Name;
            cultureName = string.IsNullOrEmpty(cultureName) ? "en-US" : cultureName;

            var response = await Client.Search(search, filter.Page * filter.Limit, filter.Limit, cultureName, accessInfo);
            return response.Select(p => _pieMapping.ToApiPatient(p)).ToList();
        }

        public async Task<List<Patient>> Search(PatientDetailsSearchFilterViewModel filter)
        {
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();
            var search = _pieMapping.GetDetailsSearchFields(filter);

            // TODO - get valid culture (either system configuration or passed in via caller)
            var cultureName = CultureInfo.CurrentCulture.Name;
            cultureName = string.IsNullOrEmpty(cultureName) ? "en-US" : cultureName;

            var response = await Client.Search(search, filter.Page * filter.Limit, filter.Limit, cultureName, accessInfo);
            return response.Select(p => _pieMapping.ToApiPatient(p)).ToList();
        }
    }
}
