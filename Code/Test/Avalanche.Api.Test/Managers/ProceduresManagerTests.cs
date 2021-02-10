using AutoMapper;
using Avalanche.Api.Managers.Procedures;
using Avalanche.Api.MappingConfigurations;
using Ism.SystemState.Client;
using Ism.SystemState.Models.Procedure;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Avalanche.Api.Test.Managers
{
    [TestFixture()]
    public class ProceduresManagerTests
    {
        IMapper _mapper;

        [SetUp]
        public void Setup()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ProceduresMappingConfiguration());
            });

            _mapper = config.CreateMapper();
        }

        [Test]
        public async Task TestGetActiveProcedureReturnsProcedure()
        {
            var stateClient = new Mock<IStateClient>();

            stateClient.Setup(s => s.GetData<ActiveProcedureState>()).ReturnsAsync(
                new ActiveProcedureState(
                    new Patient() { LastName = "name" },
                    new List<ProcedureImage>(),
                    new List<ProcedureVideo>(),
                    "libId",
                    "repId",
                    "path",
                    null,
                    null,
                    null,
                    false,
                    DateTimeOffset.UtcNow,
                    TimeZoneInfo.Local.Id));

            var manager = new ProceduresManager(stateClient.Object, _mapper);

            var result = await manager.GetActiveProcedure();

            Assert.NotNull(result);
            Assert.AreEqual("name", result.Patient?.LastName);
        }

    }
}
