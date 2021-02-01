using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Services.Media;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Ism.Common.Core.Configuration.Models;
using Ism.PgsTimeout.Client.V1;
using Ism.Security.Grpc.Interfaces;
using Ism.SystemState.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Ism.PgsTimeout.V1.Protos.PgsTimeout;
using static Ism.Utility.Core.Preconditions;

namespace Avalanche.Api.Managers.PgsTimeout
{
    public class PgsTimeoutManager : IPgsTimeoutManager
    {
        // used to get pgs configuration
        private readonly IStorageService _storageService;

        // used internally to route video and store current routes
        private readonly IRoutingService _routingService;

        // used for persisting and publishing the checkbox state for the pgs displays
        private readonly IStateClient _stateClient;

        // gRPC client for the pgs timeout application
        private readonly IPgsTimeoutService _pgsTimeoutService;

        public PgsTimeoutManager(
            IStorageService storageService, 
            IRoutingService routingService, 
            IStateClient stateClient, 
            IPgsTimeoutService pgsTimeoutService)
        {
            _storageService = ThrowIfNullOrReturn(nameof(storageService), storageService);
            _routingService = ThrowIfNullOrReturn(nameof(routingService), routingService);
            _stateClient = ThrowIfNullOrReturn(nameof(stateClient), stateClient);
            _pgsTimeoutService = ThrowIfNullOrReturn(nameof(pgsTimeoutService), pgsTimeoutService);
        }

        public async Task StartPgs()
        {
            var config = await GetConfig();
        }

        public Task StopPgs()
        {
            throw new NotImplementedException();
        }

        #region PgsTimeoutPlayer methods

        public Task<IList<string>> GetPgsVideoFiles()
        {
            throw new NotImplementedException();
        }

        public Task SetPgsVideoFile(string path)
        {
            throw new NotImplementedException();
        }

        public Task SetPlaybackPosition(double position)
        {
            throw new NotImplementedException();
        }

        public Task SetPgsVolume(double volume)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region API/routing methods

        public Task<IList<Output>> GetPgsOutputs()
        {
            throw new NotImplementedException();
        }

        public Task<List<Output>> GetTimeoutOutputs()
        {
            throw new NotImplementedException();
        }

        public Task SetPgsStateForDisplay(AliasIndexApiModel displayId, bool enabled)
        {
            // pgs checkbox state must persist reboots
            // state client should handle this
            throw new NotImplementedException();
        }

        public Task<object> GetPgsStateForAllDisplays()
        {
            // state client
            throw new NotImplementedException();
        }

        #endregion

        private async Task<PgsTimeoutConfig> GetConfig()
        {
            return await _storageService.GetJsonObject<PgsTimeoutConfig>(nameof(PgsTimeoutConfig), 1, ConfigurationContext.FromEnvironment());
        }

    }
}
