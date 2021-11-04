using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Managers.Medpresence;
using Avalanche.Api.Mapping;
using Avalanche.Api.Services.Medpresence;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Ism.MP.V1.Protos;
using Ism.SystemState.Client;
using Ism.SystemState.Models.Medpresence;
using Moq;
using NUnit.Framework;

namespace Avalanche.Api.Test.Managers
{
    [TestFixture]
    public class MedpresenceMangerTests
    {
        private IMapper? _mapper;
        private  Mock<IStateClient>? _stateClient;
        private  Mock<IMedpresenceService>? _medpresence;
        private  MedpresenceManager? _manager;

        [SetUp]
        public void Setup()
        {
            _stateClient = new Mock<IStateClient>();
            _medpresence = new Mock<IMedpresenceService>();

            var config = new MapperConfiguration(cfg => cfg.AddProfile(new MedpresenceMappingConfiguration()));
            _mapper = config.CreateMapper();

            _manager = new MedpresenceManager(_medpresence.Object, _stateClient.Object, _mapper);
        }

        [Test]
        public async Task GetMedpresenceStateShouldReturnViewModel()
        {
            _ = _stateClient!.Setup(s => s.GetData<MedpresenceState>()).ReturnsAsync(new MedpresenceState
            {
                State = "service",
                SessionId = 12345,
                Attendees = new List<string>() { "saibal" },
                IsRecording = true,
                ClipCount = 101,
                ImageCount = 101
            });

            var result = await _manager!.GetMedpresenceStateAsync().ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.IsInstanceOf<MedpresenceStateViewModel>(result);
            Assert.AreEqual(12345, result.SessionId);
        }

        [Test]
        public async Task ArchiveSessionShouldHandleNullableValues()
        {
            var request = new ArchiveServiceViewModel
            {
                SessionId = 1234,
                Title = "Hello",
                Description = "World!",
                Physician = null,
                Department = new DepartmentModel
                {
                    Id = 1,
                    Name = "Cardiology"
                },
                ProcedureType = null
            };

            await _manager!.ArchiveSessionAsync(request).ConfigureAwait(false);

            _medpresence!.Verify(x => x.ArchiveSessionAsync(It.Is<ArchiveSessionRequest>(arg => arg.SessionId == 1234)));
            _medpresence!.Verify(x => x.ArchiveSessionAsync(It.Is<ArchiveSessionRequest>(arg => arg.Physician == null)));
            _medpresence!.Verify(x => x.ArchiveSessionAsync(It.Is<ArchiveSessionRequest>(arg => arg.Department.Id == 1)));
            _medpresence!.Verify(x => x.ArchiveSessionAsync(It.Is<ArchiveSessionRequest>(arg => arg.Department.Name == "Cardiology")));
            _medpresence!.Verify(x => x.ArchiveSessionAsync(It.Is<ArchiveSessionRequest>(arg => arg.ProcedureType == null)));
        }
    }
}
