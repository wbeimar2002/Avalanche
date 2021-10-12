using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Services.Media;
using Avalanche.Shared.Domain.Models.Media;
using Avalanche.Shared.Domain.Models.Presets;
using Ism.Common.Core.Configuration.Models;
using Ism.Routing.V1.Protos;
using Newtonsoft.Json;
using static Ism.Utility.Core.Preconditions;

namespace Avalanche.Api.Managers.Presets
{
    public class PresetManager : IPresetManager
    {
        private readonly IMapper _mapper;
        private readonly IRoutingService _routingService;
        private readonly IStorageService _storageService;
        private readonly ConfigurationContext _configurationContext;

        private const string PRESETS = "presets";
        private const string SITEID = "Avalanche"; // TODO how to get Site Id

        public PresetManager(IRoutingService routingService, IStorageService storageService, IMapper mapper)
        {
            _mapper = ThrowIfNullOrReturn(nameof(mapper), mapper);
            _routingService = ThrowIfNullOrReturn(nameof(routingService), routingService);
            _storageService = ThrowIfNullOrReturn(nameof(storageService), storageService);

            _configurationContext = new ConfigurationContext { SiteId = SITEID, IdnId = Guid.NewGuid().ToString() };
        }

        public async Task<UserPresetsModel> GetPresets(string userId)
        {
            var userPresets = await _storageService.GetJsonObject<PresetsModel>(PRESETS, 1, _configurationContext);

            if (!userPresets.Users.ContainsKey(userId))
                throw new ArgumentException($"user presets for {userId} does not exists");

            return userPresets.Users[userId];
        }

        public async Task ApplyPreset(string userId, int index)
        {
            var presets = await GetPresets(userId);

            if (!presets.RoutingPresets.ContainsKey(index))
                throw new ArgumentException($"index {index} not exists in list of presets");

            var routingPresetModel = presets.RoutingPresets[index];
            var request = new RouteVideoBatchRequest();
            foreach (var route in routingPresetModel.Routes)
            {
                request.Routes.Add(new RouteVideoRequest()
                {
                    Sink = _mapper.Map<AliasIndexModel, AliasIndexMessage>(route.Sink),
                    Source = _mapper.Map<AliasIndexModel, AliasIndexMessage>(route.Source),
                });
            }

            // Route video batch
            await _routingService.RouteVideoBatch(request);
        }

        public async Task SavePreset(string userId, int index, string name)
        {
            // Get current routes from routing service
            var currentRoutes = await _routingService.GetCurrentRoutes();
            var currentRouteModels = currentRoutes.Routes.Select(x => new RouteModel 
            { 
                Sink = _mapper.Map<AliasIndexMessage,AliasIndexModel>(x.Sink), 
                Source = _mapper.Map<AliasIndexMessage, AliasIndexModel>(x.Source) 
            });

            var routingPresetModel = new RoutingPresetModel { Id = index, Name = name, Routes = currentRouteModels.ToList() };

            var userPresetModel = new UserPresetsModel();
            userPresetModel.RoutingPresets[index] = routingPresetModel;

            var presetsModel = new PresetsModel();
            presetsModel.Users[userId] = userPresetModel;

            var result = JsonConvert.SerializeObject(presetsModel);

            await _storageService.SaveJsonMetadata(PRESETS, result, 1, _configurationContext);
        }

        public async Task RemovePreset(string userId, int index)
        {
            var userPresets = await GetPresets(userId);

            if (!userPresets.RoutingPresets.ContainsKey(index))
                throw new ArgumentException($"index {index} not exists in list of presets");

            await _storageService.UpdateConfiguration<PresetsModel>(PRESETS, 1, _configurationContext, (update) =>
            {
                update.Remove(config => config.Users[userId].RoutingPresets[index]);
            });
        }
    }
}
