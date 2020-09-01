using Avalanche.Shared.Infrastructure.Services.Settings;
using Ism.IsmLogCommon.Core;
using Ism.PatientInfoEngine.Common.Core;
using Ism.Security.Grpc.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Ism.Storage.Common.Core.PatientList.Proto;
using Moq;
using Grpc.Core.Testing;
using Grpc.Core;
using System.Threading;
using Google.Protobuf.WellKnownTypes;
using Avalanche.Api.Utilities;
using Ism.PatientInfoEngine.Common.Core.Protos;
using System.Runtime.ConstrainedExecution;

namespace Avalanche.Api.Services.Health
{
    public class PieService : IPieService
    {
        readonly IConfigurationService _configurationService;
        readonly string _hostIpAddress;

        public bool IgnoreGrpcServicesMocks { get; set; }

        public PatientListService.PatientListServiceClient PatientListServiceClient { get; set; }
        public PatientListStorage.PatientListStorageClient PatientListStorageClient { get; set; }

        public PieService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
            _hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");

            var pieServiceGrpcPort = _configurationService.GetEnvironmentVariable("pieServiceGrpcPort");
            var storageServiceGrpcPort = _configurationService.GetEnvironmentVariable("storageServiceGrpcPort");

            var grpcCertificate = _configurationService.GetEnvironmentVariable("grpcCertificate");
            var grpcPassword = _configurationService.GetEnvironmentVariable("grpcPassword");

            var certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(grpcCertificate, grpcPassword);

            PatientListServiceClient = ClientHelper.GetInsecureClient<PatientListService.PatientListServiceClient>($"https://{_hostIpAddress}:{pieServiceGrpcPort}");
            PatientListStorageClient = ClientHelper.GetInsecureClient<PatientListStorage.PatientListStorageClient>($"https://{_hostIpAddress}:{storageServiceGrpcPort}");

            //PatientListServiceClient = ClientHelper.GetSecureClient<PatientListService.PatientListServiceClient>($"https://{_hostIpAddress}:{patientListServiceGrpcPort}", certificate);
            //PatientListStorageClient = ClientHelper.GetSecureClient<PatientListStorage.PatientListStorageClient>($"https://{_hostIpAddress}:{patientListStorageGrpcPort}", certificate);
        }

        public async Task<List<Ism.PatientInfoEngine.Common.Core.Protos.PatientRecordMessage>> Search(PatientSearchFieldsMessage searchFields, int firstRecordIndex, int maxResults, string searchCultureName, Ism.PatientInfoEngine.Common.Core.Protos.AccessInfoMessage accessInfoMessage)
        {
            var request = new SearchRequest
            {
                FirstRecordIndex = firstRecordIndex,
                MaxResults = maxResults,
                SearchCultureName = searchCultureName,
                AccessInfo = accessInfoMessage,
                SearchFields = searchFields
            };

            var response = await PatientListServiceClient.SearchAsync(request);
            return response?.UpdatedPatList.ToList();
        }

        public async Task<Ism.Storage.Common.Core.PatientList.Proto.PatientRecordMessage> RegisterPatient(Ism.Storage.Common.Core.PatientList.Proto.PatientRecordMessage patientRecordMessage, Ism.Storage.Common.Core.PatientList.Proto.AccessInfoMessage accessInfoMessage)
        {
            var response = await PatientListStorageClient.AddPatientRecordAsync(new AddPatientRecordRequest()
            {
                AccessInfo = accessInfoMessage,
                PatientRecord = patientRecordMessage
            });

            return response.PatientRecord;
        }

        public async Task UpdatePatient(Ism.Storage.Common.Core.PatientList.Proto.PatientRecordMessage patientRecordMessage, Ism.Storage.Common.Core.PatientList.Proto.AccessInfoMessage accessInfoMessage)
        {
            var response = await PatientListStorageClient.UpdatePatientRecordAsync(new UpdatePatientRecordRequest()
            {
                AccessInfo = accessInfoMessage,
                PatientRecord = patientRecordMessage
            });
        }

        public async Task<int> DeletePatient(ulong patiendId, Ism.Storage.Common.Core.PatientList.Proto.AccessInfoMessage accessInfoMessage)
        {
            var response = await PatientListStorageClient.DeletePatientRecordAsync(new DeletePatientRecordRequest()
            {
                AccessInfo = accessInfoMessage,
                PatientRecordId = patiendId
            });

            return response.RecordsDeleted;
        }
    }
}
