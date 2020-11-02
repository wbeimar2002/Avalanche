using AutoMapper;
using Avalanche.Api.Managers.Metadata;
using Avalanche.Api.MappingConfigurations;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Health;
using Avalanche.Shared.Infrastructure.Services.Settings;
using Ism.Security.Grpc.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Api.Tests.Managers
{
    [TestFixture()]
    public class MetadataManagerTests
    {
        Mock<IStorageService> _storageService;
        Mock<ISettingsService> _settingsService;
        Mock<IDataManagementService> _dataManagementService;

        IMapper _mapper;
        MetadataManager _manager;

        [SetUp]
        public void Setup()
        {
            _storageService = new Mock<IStorageService>();
            _settingsService = new Mock<ISettingsService>();
            _dataManagementService = new Mock<IDataManagementService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new PieMappingConfigurations());
            });

            _mapper = config.CreateMapper();
            _manager = new MetadataManager(_storageService.Object, _dataManagementService.Object, _settingsService.Object, _mapper);
        }

        /*
        Task<AddDepartmentResponse> AddDepartment(AddDepartmentRequest request);
        Task<AddProcedureTypeResponse> AddProcedureType(AddProcedureTypeRequest request);
        Task DeleteDepartment(DeleteDepartmentRequest request);
        Task DeleteProcedureType(DeleteProcedureTypeRequest request);
        Task<GetDepartmentsResponse> GetAllDepartments();
        Task<GetProceduresByDepartmentResponse> GetProceduresByDepartment(GetProceduresByDepartmentRequest request);
         */
    }
}
