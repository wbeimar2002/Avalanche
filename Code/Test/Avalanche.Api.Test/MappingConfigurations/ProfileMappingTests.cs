using AutoMapper;
using Avalanche.Api.Mapping;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models.Media;
using Ism.Routing.V1.Protos;
using Ism.SystemState.Models.Procedure;
using NUnit.Framework;
using System;
using System.Collections.Generic;

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

        [Test]
        public void TestActiveProcedureStateToActiveProcedureViewModel()
        {
            var activeProcedure = new ActiveProcedureState(
                    new Patient() { LastName = "name" },
                    new List<ProcedureImage>() { new ProcedureImage(Guid.NewGuid(), "source", "channel", false, "path", "path", null, DateTimeOffset.UtcNow, Guid.NewGuid()) },
                    new List<ProcedureVideo>(),
                    new List<ProcedureVideo>(),
                    "libId",
                    "repId",
                    "path",
                    null,
                    new ProcedureType() { Id = 1, Name = "TestProceType" },
                    null,
                    false,
                    DateTimeOffset.UtcNow,
                    TimeZoneInfo.Local.Id,
                    false,
                    new List<ProcedureNote>(),
                    null,
                    null,
                    null,
                    new List<VideoRecordingEvent>(),
                    BackgroundRecordingMode.StartImmediately)
            {
                IsRecording = true,
                IsBackgroundRecording = true
            };

            var stateViewModel = _mapper.Map<ActiveProcedureViewModel>(activeProcedure);

            Assert.AreEqual(activeProcedure.IsBackgroundRecording, stateViewModel.IsBackgroundRecording);
            Assert.AreEqual(activeProcedure.IsRecording, stateViewModel.IsRecording);
            Assert.AreEqual(activeProcedure.Videos.Count, stateViewModel.Videos.Count);
            Assert.AreEqual(activeProcedure.Images.Count, stateViewModel.Images.Count);
            Assert.AreEqual(activeProcedure.BackgroundVideos.Count, stateViewModel.BackgroundVideos.Count);
        }

        [Test]
        public void TestTileLayoutMessageToTileLayoutModel()
        {
            var viewPorts = new Google.Protobuf.Collections.RepeatedField<TileViewportMessage>()
            {
                new TileViewportMessage { Layer = 1, Height = 10, Width = 10, X = 10, Y = 10 },
                new TileViewportMessage { Layer = 1, Height = 100, Width = 100, X = 10, Y = 10 },
            };

            var layoutMessage = new TileLayoutMessage()
            {
                LayoutName = "Test",
                NumViewports = viewPorts.Count
            };
            layoutMessage.Viewports.AddRange(viewPorts);

            var tileLayoutModel = _mapper.Map<TileLayoutModel>(layoutMessage);

            Assert.AreEqual(layoutMessage.LayoutName, tileLayoutModel.LayoutName);
            Assert.AreEqual(layoutMessage.Viewports.Count, tileLayoutModel?.ViewPorts?.Count);
        }

        [Test]
        public void TestTileVideoRouteMessageToTileVideoRouteModel()
        {
            var sources = new Google.Protobuf.Collections.RepeatedField<AliasIndexMessage>()
            {
                new AliasIndexMessage { Alias ="test1", Index = "test1"},
                new AliasIndexMessage {Alias ="test2", Index = "test2"}
            };

            var route = new TileVideoRouteMessage()
            {
                LayoutName = "Test",
                Sink = new AliasIndexMessage { Alias ="test", Index = "test"},
                SourceCount = sources.Count
            };

            route.Sources.AddRange(sources);

            var tileRouteModel = _mapper.Map<TileVideoRouteModel>(route);

            Assert.AreEqual(route.LayoutName, tileRouteModel.LayoutName);
            Assert.AreEqual(route.Sink.Alias, tileRouteModel.Sink?.Alias);
            Assert.AreEqual(route.Sink.Index, tileRouteModel.Sink?.Index);
            Assert.AreEqual(route.SourceCount, tileRouteModel.SourceCount);
            Assert.AreEqual(route.Sources.Count, tileRouteModel.Sources.Count);
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
