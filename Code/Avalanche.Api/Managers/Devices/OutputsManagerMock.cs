using AutoFixture;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Devices
{
    public class OutputsManagerMock : IOutputsManager
    {
        public Task<List<Output>> GetAllAvailable()
        {
            var fixture = new Fixture();
            return Task.FromResult(fixture.CreateMany<Output>(10).ToList());
        }

        public Task<Signal> GetContent(string contentType)
        {
            var fixture = new Fixture();
            return Task.FromResult(fixture.Create<Signal>());
        }

        public Task<State> GetCurrentState(string id, StateTypes commandType)
        {
            var fixture = new Fixture();
            return Task.FromResult(fixture.Create<State>());
        }

        public Task SendCommand(Command command)
        {
            return Task.CompletedTask;
        }
    }
}
