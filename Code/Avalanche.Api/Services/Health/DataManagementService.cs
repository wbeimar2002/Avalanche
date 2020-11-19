using Avalanche.Shared.Infrastructure.Services.Settings;
using Google.Protobuf.WellKnownTypes;
using Ism.Security.Grpc.Interfaces;
using Ism.Storage.Core.DataManagement.V1.Protos;
using Ism.Storage.DataManagement.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Ism.Storage.Core.DataManagement.V1.Protos.DataManagementStorage;

namespace Avalanche.Api.Services.Health
{
    public class DataManagementService : IDataManagementService
    {
        readonly IConfigurationService _configurationService;
        readonly string _hostIpAddress;
        readonly string _dataManagementGrpcPort;

        DataManagementStorageSecureClient DataManagementStorageClient { get; set; }

        public DataManagementService(
            IGrpcClientFactory<DataManagementStorageClient> grpcClientFactory,
            ICertificateProvider certificateProvider,
            IConfigurationService configurationService)
        {
            _configurationService = configurationService;

            _hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");
            _dataManagementGrpcPort = _configurationService.GetEnvironmentVariable("dataManagementGrpcPort");

            DataManagementStorageClient = new DataManagementStorageSecureClient(grpcClientFactory, _hostIpAddress, _dataManagementGrpcPort, certificateProvider);
        }

        public async Task<AddDepartmentResponse> AddDepartment(AddDepartmentRequest request) => await DataManagementStorageClient.AddDepartment(request);

        public async Task<AddProcedureTypeResponse> AddProcedureType(AddProcedureTypeRequest request) => await DataManagementStorageClient.AddProcedureType(request);

        public async Task DeleteDepartment(DeleteDepartmentRequest request) => await DataManagementStorageClient.DeleteDepartment(request);

        public async Task DeleteProcedureType(DeleteProcedureTypeRequest request) => await DataManagementStorageClient.DeleteProcedureType(request);

        public async Task<GetDepartmentsResponse> GetAllDepartments() => await DataManagementStorageClient.GetAllDepartments(new Empty());

        public async Task<GetProceduresByDepartmentResponse> GetProceduresByDepartment(GetProceduresByDepartmentRequest request) => await DataManagementStorageClient.GetProceduresByDepartment(request);

        public async Task<ProcedureTypeMessage> GetProcedureTypeAsync(ProcedureTypeMessage request) => await DataManagementStorageClient.GetProcedureTypeAsync(request);
    }
}
