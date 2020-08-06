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
        readonly IAccessInfoFactory _accessInfoFactory;
        readonly string _hostIpAddress;

        public bool IgnoreGrpcServicesMocks { get; set; }

        public PatientListService.PatientListServiceClient PatientListServiceClient { get; set; }
        public PatientListStorage.PatientListStorageClient PatientListStorageClient { get; set; }

        public PieService(IConfigurationService configurationService, IAccessInfoFactory accessInfoFactory)
        {
            _configurationService = configurationService;
            _accessInfoFactory = accessInfoFactory;
            _hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");

            var patientListServiceGrpcPort = _configurationService.GetEnvironmentVariable("PatientListServiceGrpcPort");
            var patientListStorageGrpcPort = _configurationService.GetEnvironmentVariable("PatientListStorageGrpcPort");

            var grpcCertificate = _configurationService.GetEnvironmentVariable("grpcCertificate");
            var grpcPassword = _configurationService.GetEnvironmentVariable("grpcPassword");

            var certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(grpcCertificate, grpcPassword);

            //PatientListServiceClient = ClientHelper.GetInsecureClient<PatientListService.PatientListServiceClient>($"https://{_hostIpAddress}:{patientListServiceGrpcPort}");
            //PatientListStorageClient = ClientHelper.GetInsecureClient<PatientListStorage.PatientListStorageClient>($"https://{_hostIpAddress}:{patientListStorageGrpcPort}");

            PatientListServiceClient = ClientHelper.GetSecureClient<PatientListService.PatientListServiceClient>($"https://{_hostIpAddress}:{patientListServiceGrpcPort}", certificate);
            PatientListStorageClient = ClientHelper.GetSecureClient<PatientListStorage.PatientListStorageClient>($"https://{_hostIpAddress}:{patientListStorageGrpcPort}", certificate);
        }

        public async Task<List<Patient>> Search(PatientSearchFieldsMessage searchFields, int firstRecordIndex, int maxResults, string searchCultureName)
        {
            try
            {
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

                var accessInfo = _accessInfoFactory.GenerateAccessInfo();

                var accessInfoMessage = new Ism.PatientInfoEngine.Common.Core.Protos.AccessInfoMessage
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

                var response = await PatientListServiceClient.SearchAsync(request);
                return response?.UpdatedPatList.Select(pieRecord => new Patient()
                {
                    AccessionNumber = pieRecord.AccessionNumber,
                    DateOfBirth = new DateTime(pieRecord.Patient.Dob.Year, pieRecord.Patient.Dob.Month, pieRecord.Patient.Dob.Day),
                    Department = pieRecord.Department,
                    Gender = pieRecord.Patient.Sex.ToString(),
                    Id = pieRecord.InternalId,
                    MRN = pieRecord.MRN,
                    LastName = pieRecord.Patient.LastName,
                    FirstName = pieRecord.Patient.FirstName,
                    ProcedureType = pieRecord.ProcedureType,
                    Room = pieRecord.Room
                }).ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Patient> RegisterPatient(Patient newPatient)
        {
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();

            var accessInfoMessage = GrpcModelsMappingHelper.GetAccessInfoMessage(accessInfo);

            var response = await PatientListStorageClient.AddPatientRecordAsync(new AddPatientRecordRequest()
            {
                AccessInfo = accessInfoMessage,
                PatientRecord = new Ism.Storage.Common.Core.PatientList.Proto.PatientRecordMessage()
                {
                    Mrn = newPatient.MRN,
                    Patient = new Ism.Storage.Common.Core.PatientList.Proto.PatientMessage()
                    {
                        Dob = new Ism.Storage.Common.Core.PatientList.Proto.FixedDateMessage()
                        {
                            Day = newPatient.DateOfBirth.Day,
                            Month = newPatient.DateOfBirth.Month,
                            Year = newPatient.DateOfBirth.Year
                        },
                        FirstName = newPatient.FirstName,
                        LastName = newPatient.LastName,
                        Sex = GrpcModelsMappingHelper.GetGender(newPatient.Gender),
                    }
                }
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
                FirstName = pieRecord.Patient.FirstName,
                ProcedureType = pieRecord.ProcedureType,
                Room = pieRecord.Room
            };
        }

        public async Task UpdatePatient(Patient existingPatient)
        {
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();

            var accessInfoMessage = GrpcModelsMappingHelper.GetAccessInfoMessage(accessInfo);

            var response = await PatientListStorageClient.UpdatePatientRecordAsync(new UpdatePatientRecordRequest()
            {
                AccessInfo = accessInfoMessage,
                PatientRecord = new Ism.Storage.Common.Core.PatientList.Proto.PatientRecordMessage() {
                    Mrn = existingPatient.MRN,
                    Patient = new Ism.Storage.Common.Core.PatientList.Proto.PatientMessage()
                    {
                        Dob = new Ism.Storage.Common.Core.PatientList.Proto.FixedDateMessage()
                        {
                            Day = existingPatient.DateOfBirth.Day,
                            Month = existingPatient.DateOfBirth.Month,
                            Year = existingPatient.DateOfBirth.Year
                        },
                        FirstName = existingPatient.FirstName,
                        LastName = existingPatient.LastName,
                        Sex = GrpcModelsMappingHelper.GetGender(existingPatient.Gender),
                    }
                }
            });
        }

        public async Task<int> DeletePatient(ulong patiendId)
        {
            var accessInfo = _accessInfoFactory.GenerateAccessInfo();

            var accessInfoMessage = GrpcModelsMappingHelper.GetAccessInfoMessage(accessInfo);

            var response = await PatientListStorageClient.DeletePatientRecordAsync(new DeletePatientRecordRequest()
            {
                AccessInfo = accessInfoMessage,
                PatientRecordId = patiendId
            });

            return response.RecordsDeleted;
        }
    }
}
