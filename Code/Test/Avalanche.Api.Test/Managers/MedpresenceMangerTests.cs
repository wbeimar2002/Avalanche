using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Managers.Medpresence;
using Avalanche.Api.Services.Medpresence;
using Avalanche.Api.ViewModels;
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
        private  Mock<IStateClient> _stateClient;
        private  Mock<IMedpresenceService> _medpresence;
        private  MedpresenceManager? _manager;

        [SetUp]
        public void Setup()
        {
            _stateClient = new Mock<IStateClient>();
            _medpresence = new Mock<IMedpresenceService>();

            _mapper = new MapperConfiguration(cfg => cfg.CreateMap<MedpresenceState, MedpresenceStateViewModel>()).CreateMapper();
            _manager = new MedpresenceManager(_medpresence.Object, _stateClient.Object, _mapper);
        }

        [Test]
        public async Task GetMedpresenceStateShouldReturnViewModel()
        {
            _ = _stateClient.Setup(s => s.GetData<MedpresenceState>()).ReturnsAsync(new MedpresenceState
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
    }
}
