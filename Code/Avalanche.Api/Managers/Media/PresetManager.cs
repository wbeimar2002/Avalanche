using AutoMapper;
using Avalanche.Api.Services.Media;
using Avalanche.Shared.Domain.Models.Media;
using Ism.Routing.V1.Protos;
using System.Threading.Tasks;
using static Ism.Utility.Core.Preconditions;

namespace Avalanche.Api.Managers.Media
{
    public class PresetManager : IPresetManager
    {
        private readonly IMapper _mapper;
        private readonly IRoutingService _routingService;

        public PresetManager(IRoutingService routingService, IMapper mapper)
        {
            _mapper = ThrowIfNullOrReturn(nameof(mapper), mapper);
            _routingService = ThrowIfNullOrReturn(nameof(routingService), routingService);
        }

        public async Task ApplyPreset(RoutingPresetModel presetViewModel)
        {
            // Route videos as per preset
            foreach(var route in presetViewModel.Routes)
            {
                await _routingService.RouteVideo(new RouteVideoRequest()
                {
                    Sink = _mapper.Map<AliasIndexModel, AliasIndexMessage>(route.Sink),
                    Source = _mapper.Map<AliasIndexModel, AliasIndexMessage>(route.Source),
                });
            }
        }
    }
}
