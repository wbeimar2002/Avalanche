using AutoMapper;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Services.Settings;
using Ism.Common.Core.Configuration.Models;
using Ism.Security.Grpc.Interfaces;
using Ism.Storage.Core.DataManagement.V1.Protos;
using Ism.Storage.DataManagement.Client;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Ism.Storage.Core.DataManagement.V1.Protos.DataManagementStorage;

namespace Avalanche.Api.Managers.Metadata
{
    public class MetadataManager : IMetadataManager
    {
        readonly IStorageService _storageService;
        readonly ISettingsService _settingsService;
        readonly IMapper _mapper;

        readonly IConfigurationService _configurationService;
        readonly string _hostIpAddress;
        readonly string _dataManagementGrpcPort;

        DataManagementStorageSecureClient DataManagementStorageClient { get; set; }


        public MetadataManager(IStorageService storageService,
            IGrpcClientFactory<DataManagementStorageClient> grpcClientFactory, 
            ICertificateProvider certificateProvider,
            IConfigurationService configurationService,
            ISettingsService settingsService,
            IMapper mapper)
        {
            _configurationService = configurationService;
            _storageService = storageService;
            _settingsService = settingsService;
            _mapper = mapper;

            _hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");
            _dataManagementGrpcPort = _configurationService.GetEnvironmentVariable("dataManagementGrpcPort");

            DataManagementStorageClient = new DataManagementStorageSecureClient(grpcClientFactory, _hostIpAddress, _dataManagementGrpcPort, certificateProvider);
        }

        public async Task<List<KeyValuePairViewModel>> GetMetadata(Shared.Domain.Enumerations.MetadataTypes type, Avalanche.Shared.Domain.Models.User user)
        {
            /*
             *  The user shall be warned if they attempt to Add a Department with a duplicate name.
                Deleting a Department shall also Delete all related Procedure Types for that Department.
                When viewed on the front end Department List shall be displayed in alphabetical order.  Does not require sorting on the backend.

                Procedure Type, optionally, may have a child relationship to a Department.  If no Department is selected then Procedure Types without a Department are shown.
                Procedure Type.Name must be unique for the related Department.  
                The user shall be warned if they attempt to enter a Procedure Type with a duplicate name.
             */
            var configurationContext = _mapper.Map<Avalanche.Shared.Domain.Models.User, ConfigurationContext>(user);

            switch (type)
            {
                case Shared.Domain.Enumerations.MetadataTypes.Sex:
                    return (await _storageService.GetJson<ListContainerViewModel>("SexTypes", 1, configurationContext)).Items;
                case Shared.Domain.Enumerations.MetadataTypes.ContentTypes:
                    return (await _storageService.GetJson<ListContainerViewModel>("ContentTypes", 1, configurationContext)).Items;
                case Shared.Domain.Enumerations.MetadataTypes.SourceTypes:
                    return (await _storageService.GetJson<ListContainerViewModel>("SourceTypes", 1, configurationContext)).Items;
                default:
                    return new List<KeyValuePairViewModel>();
            }
        }

        public async Task<Department> AddDepartment(Department department)
        {
            var result = await DataManagementStorageClient.AddDepartment(new Ism.Storage.Core.DataManagement.V1.Protos.AddDepartmentRequest());
            return _mapper.Map<AddDepartmentResponse, Department>(result);
        }

        public async Task DeleteDepartment(string departmentName)
        {
            await DataManagementStorageClient.DeleteDepartment(new Ism.Storage.Core.DataManagement.V1.Protos.DeleteDepartmentRequest());
        }

        public async Task<List<Department>> GetAllDepartments()
        {
            
            var result = await DataManagementStorageClient.GetAllDepartments(new Google.Protobuf.WellKnownTypes.Empty());

            //TODO: Validate Why not order this? When viewed on the front end Department List shall be displayed in alphabetical order.  
            //Does not require sorting on the backend.
            return _mapper.Map<IList<DepartmentMessage>, IList<Department>>(result.DepartmentList)
                .OrderBy(d => d.Name).ToList();
        }

        public async Task<ProcedureType> AddProcedureType(ProcedureType procedureType)
        {
            var result = await DataManagementStorageClient.AddProcedureType(new Ism.Storage.Core.DataManagement.V1.Protos.AddProcedureTypeRequest());
            return _mapper.Map<AddProcedureTypeResponse, ProcedureType>(result);
        }

        public async Task DeleteProcedureType(Avalanche.Shared.Domain.Models.User user, string procedureTypeName, string departmentName = null)
        {
            await ValidateDepartmentSupport(user, departmentName);

            await DataManagementStorageClient.DeleteProcedureType(new Ism.Storage.Core.DataManagement.V1.Protos.DeleteProcedureTypeRequest());
        }

        public async Task<List<ProcedureType>> GetProceduresByDepartment(Avalanche.Shared.Domain.Models.User user, string departmentName = null)
        {
            await ValidateDepartmentSupport(user, departmentName);

            var result = await DataManagementStorageClient.GetProceduresByDepartment(new Ism.Storage.Core.DataManagement.V1.Protos.GetProceduresByDepartmentRequest()
            {
                DepartmentName = departmentName
            });

            return _mapper.Map<IList<ProcedureTypeMessage>, IList<ProcedureType>>(result.ProcedureTypeList).ToList();
        }

        private async Task ValidateDepartmentSupport(User user, string departmentName)
        {
            var configurationContext = _mapper.Map<Avalanche.Shared.Domain.Models.User, ConfigurationContext>(user);
            var setupSettings = await _settingsService.GetSetupSettingsAsync(configurationContext);

            //TODO: Check the strategy to throw business logic exceptions
            if (setupSettings.DepartmentsSupported && string.IsNullOrEmpty(departmentName))
                throw new System.Exception("Departments value is invalid");

            if (!setupSettings.DepartmentsSupported && !string.IsNullOrEmpty(departmentName))
                throw new System.Exception("Departments is not supported");
        }
    }
}