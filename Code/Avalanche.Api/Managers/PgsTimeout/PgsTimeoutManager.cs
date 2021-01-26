using Avalanche.Api.Services.Maintenance;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Ism.Utility.Core.Preconditions;

namespace Avalanche.Api.Managers.PgsTimeout
{
    public class PgsTimeoutManager : IPgsTimeoutManager
    {
        private readonly IStorageService _storageService;

        public PgsTimeoutManager(IStorageService storageService)
        {
            _storageService = ThrowIfNullOrReturn(nameof(storageService), storageService);

            var config = _storageService.GetJsonObject<PgsTimeoutConfig>(nameof(PgsTimeoutConfig), 1, null).Result;
        }

        public Task StartPgs()
        {
            throw new NotImplementedException();
        }

        public Task StopPgs()
        {
            throw new NotImplementedException();
        }

        #region PgsTimeoutPlayer methods

        public Task<IDictionary<string, string>> GetPgsVideoFiles()
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

        #endregion

    }
}
