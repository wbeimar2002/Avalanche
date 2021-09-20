using System.Threading.Tasks;
using Avalanche.Api.Services.Medpresence;

namespace Avalanche.Api.Managers.Medpresence
{
    public class MedpresenceManager : IMedpresenceManager
    {
        private readonly IMedpresenceService _medpresenceService;

        public MedpresenceManager(IMedpresenceService medpresenceService) => _medpresenceService = medpresenceService;

        public async Task StartServiceSession() => await _medpresenceService.StartServiceSession().ConfigureAwait(false);
        public async Task StopServiceSession() => await _medpresenceService.StopServiceSession().ConfigureAwait(false);
    }
}
