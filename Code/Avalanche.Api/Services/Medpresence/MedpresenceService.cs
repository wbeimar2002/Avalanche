using Ism.Common.Core.Aspects;
using Ism.Medpresence.Client.V1;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Avalanche.Api.Services.Medpresence
{
    public class MedpresenceService : IMedpresenceService
    {
        ILogger<MedpresenceService> _logger;
        MedpresenceSecureClient _medpresence;

        public MedpresenceService(ILogger<MedpresenceService> logger, MedpresenceSecureClient medpresence)
        {
            _logger = logger;
            _medpresence = medpresence;
        }

        [AspectLogger]
        public async Task StartServiceSession()
        {
            _logger.LogInformation($"Starting a service session...");
        }

        [AspectLogger]
        public async Task StopServiceSession()
        {
            _logger.LogInformation($"Stopping a service session...");
        }
    }
}
