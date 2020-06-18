using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Services.Settings;
using Ism.IsmLogCommon.Core;
using Ism.PatientInfoEngine.Common.Core;
using Ism.Security.Grpc.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Ism.Storage.Common.Core.PatientList.Proto;
using Avalanche.Api.Helpers;

namespace Avalanche.Api.Services.Health
{
    public class PieService : IPieService
    {
        readonly IConfigurationService _configurationService;
        readonly string _hostIpAddress;

        public PatientListService.PatientListServiceClient PatientListServiceClient { get; set; }
        public PatientListStorage.PatientListStorageClient PatientListStorageClient { get; set; }

        public PieService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;

            _hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");

            var patientListServiceGrpcPort = _configurationService.GetEnvironmentVariable("PatientListServiceGrpcPort");
            var patientListStorageGrpcPort = _configurationService.GetEnvironmentVariable("PatientListStorageGrpcPort");

            var grpcCertificate = _configurationService.GetEnvironmentVariable("grpcCertificate");
            var grpcPassword = _configurationService.GetEnvironmentVariable("grpcPassword");

            var certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(grpcCertificate, grpcPassword);

            PatientListServiceClient = ClientHelper.GetInsecureClient<PatientListService.PatientListServiceClient>($"https://{_hostIpAddress}:{patientListServiceGrpcPort}");
            PatientListStorageClient = ClientHelper.GetInsecureClient<PatientListStorage.PatientListStorageClient>($"https://{_hostIpAddress}:{patientListStorageGrpcPort}");
        }

        public async Task<IEnumerable<Patient>> Search(PatientSearchFieldsMessage searchFields, int firstRecordIndex, int maxResults, string searchCultureName, AccessInfo accessInfo)
        {
            var accessInfoMessage = new Ism.PatientInfoEngine.Common.Core.AccessInfoMessage()
            {
                ApplicationName = accessInfo.ApplicationName,
                Ip = accessInfo.Ip,
                Id = accessInfo.Id.ToString(),
                Details = accessInfo.Details,
                MachineName = accessInfo.MachineName,
                UserName = accessInfo.UserName
            };

            var request = new SearchRequest
            {
                FirstRecordIndex = firstRecordIndex,
                MaxResults = maxResults,
                SearchCultureName = searchCultureName,
                AccessInfo = accessInfoMessage,
                SearchFields = searchFields
            };

            var reply = await PatientListServiceClient.SearchAsync(request);
            return reply?.UpdatedPatList.Select(pieRecord => new Patient()
            {
                AccessionNumber = pieRecord.AccessionNumber,
                DateOfBirth = new DateTime(pieRecord.Patient.Dob.Year, pieRecord.Patient.Dob.Month, pieRecord.Patient.Dob.Day),
                Department = pieRecord.Department,
                Gender = pieRecord.Patient.Sex.ToString(),
                Id = pieRecord.InternalId,
                MRN = pieRecord.MRN,
                LastName = pieRecord.Patient.LastName,
                Name = pieRecord.Patient.FirstName,
                ProcedureType = pieRecord.ProcedureType,
                Room = pieRecord.Room
            });
        }

        public async Task<Patient> RegisterPatient(Patient newPatient, AccessInfo accessInfo)
        {
            var accessInfoMessage = GrpcModelsMappingHelper.GetAccessInfoMessage(accessInfo);

            //TODO: Add data
            var response = await PatientListStorageClient.AddPatientRecordAsync(new AddPatientRecordRequest()
            { 
            });

            var pieRecord = response.PatientRecord;

            return new Patient()
            {
                AccessionNumber = pieRecord.AccessionNumber,
                DateOfBirth = new DateTime(pieRecord.Patient.Dob.Year, pieRecord.Patient.Dob.Month, pieRecord.Patient.Dob.Day),
                Department = pieRecord.Department,
                Gender = pieRecord.Patient.Sex.ToString(),
                Id = pieRecord.InternalId,
                MRN = pieRecord.Mrn,
                LastName = pieRecord.Patient.LastName,
                Name = pieRecord.Patient.FirstName,
                ProcedureType = pieRecord.ProcedureType,
                Room = pieRecord.Room
            };
        }

        public async Task UpdatePatient(Patient existingPatient, AccessInfo accessInfo)
        {
            var accessInfoMessage = GrpcModelsMappingHelper.GetAccessInfoMessage(accessInfo);

            //TODO: Add data
            var response = await PatientListStorageClient.UpdatePatientRecordAsync(new UpdatePatientRecordRequest()
            {
                AccessInfo = accessInfoMessage,
            });
        }

        public async Task DeletePatient(ulong patiendId, AccessInfo accessInfo)
        {
            var accessInfoMessage = GrpcModelsMappingHelper.GetAccessInfoMessage(accessInfo);

            var response = await PatientListStorageClient.DeletePatientRecordAsync(new DeletePatientRecordRequest()
            {
                AccessInfo = accessInfoMessage,
                PatientRecordId = patiendId
            });
        }
    }
}
