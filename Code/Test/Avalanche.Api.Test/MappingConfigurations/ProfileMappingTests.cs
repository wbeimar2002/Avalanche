using AutoMapper;
using Avalanche.Api.Mapping;
using Avalanche.Api.ViewModels;
using Ism.SystemState.Models.Procedure;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Api.Tests.MappingConfigurations
{
    [TestFixture]
    public class ProfileMappingTests
    {
        IMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new PatientMappingConfiguration());
                cfg.AddProfile(new DataMappingConfiguration());
                cfg.AddProfile(new MaintenanceMappingConfiguration());
                cfg.AddProfile(new MediaMappingConfiguration());
                cfg.AddProfile(new ProceduresMappingConfiguration());
                cfg.AddProfile(new RecorderMappingConfiguration());
                cfg.AddProfile(new RoutingMappingConfiguration());
            });

            _mapper = config.CreateMapper();
        }

        [Test]
        public void PatientMappingConfiguration_IsValid()
        {
            AssertProfileIsValid<PatientMappingConfiguration>();      
        }

        [Test]
        public void DataMappingConfiguration_IsValid()
        {
            AssertProfileIsValid<DataMappingConfiguration>();
        }

        [Test]
        public void ProceduresMappingConfigurations_IsValid()
        {
            AssertProfileIsValid<ProceduresMappingConfiguration>();
        }

        [Test]
        public void MediaMappingConfiguration_IsValid()
        {
            AssertProfileIsValid<MediaMappingConfiguration>();
        }

        [Test]
        public void MaintenanceMappingConfiguration_IsValid()
        {
            AssertProfileIsValid<MaintenanceMappingConfiguration>();
        }

        [Test]
        public void RecorderMappingConfiguration_IsValid()
        {
            AssertProfileIsValid<RecorderMappingConfiguration>();
        }

        [Test]
        public void RoutingMappingConfiguration_IsValid()
        {
            AssertProfileIsValid<RoutingMappingConfiguration>();
        }

        [Test]
        public void TestPatientViewModelToStateModel()
        {
            var now = DateTime.Now;
            var viewModel = new PatientViewModel
            {
                DateOfBirth = now,
                Department = new Shared.Domain.Models.DepartmentModel { Id = 1, Name = "Dept" },
                FirstName = "First",
                Id = 2,
                LastName = "Last",
                MRN = "1234",
                Physician = new Shared.Domain.Models.PhysicianModel { Id = "3", FirstName = "f", LastName = "l" },
                ProcedureType = new Shared.Domain.Models.ProcedureTypeModel { Id = 4, DepartmentId = 1, Name = "proc" },
                Sex = new KeyValuePairViewModel { Id = "M", TranslationKey = "key", Value = "M" }
            };
            var stateModel = _mapper.Map<Patient>(viewModel);

            Assert.AreEqual(viewModel.DateOfBirth, stateModel.DateOfBirth);
            Assert.AreEqual(viewModel.FirstName, stateModel.FirstName);
            Assert.AreEqual(viewModel.Id, stateModel.Id);
            Assert.AreEqual(viewModel.LastName, stateModel.LastName);
            Assert.AreEqual(viewModel.MRN, stateModel.MRN);
            Assert.AreEqual(viewModel.Sex.Id, stateModel.Sex);
        }

        private void AssertProfileIsValid<TProfile>() where TProfile : Profile, new()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid<TProfile>();
        }

        [TearDown]
        public void TearDown()
        {

        }
    }
}
