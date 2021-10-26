using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Utilities;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Domain.Models.Media;
using Avalanche.Shared.Domain.Models.Presets;
using Ism.Common.Core.Configuration.Models;
using Ism.Routing.V1.Protos;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using static Ism.Utility.Core.Preconditions;

namespace Avalanche.Api.Managers.Presets
{
    public class PresetManager : IPresetManager
    {
        private readonly IRoutingService _routingService;
        private readonly IStorageService _storageService;

        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserModel user;
        private readonly ConfigurationContext _configurationContext;


        private const string PRESETS = "Presets";

        public PresetManager(IRoutingService routingService, IStorageService storageService, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _mapper = ThrowIfNullOrReturn(nameof(mapper), mapper);
            _routingService = ThrowIfNullOrReturn(nameof(routingService), routingService);
            _storageService = ThrowIfNullOrReturn(nameof(storageService), storageService);
            _httpContextAccessor = httpContextAccessor;

            user = HttpContextUtilities.GetUser(_httpContextAccessor.HttpContext);
            _configurationContext = _mapper.Map<UserModel, ConfigurationContext>(user);
            _configurationContext.IdnId = Guid.NewGuid().ToString();
        }

        public async Task<UserPresetsModel> GetPresets(string userId)
        {
            var userPresets = await _storageService.GetJsonObject<PresetsModel>(PRESETS, 1, _configurationContext).ConfigureAwait(false);

            if (!userPresets.Users.ContainsKey(userId))
            {
                throw new ArgumentException($"user presets for {userId} does not exists");
            }

            return userPresets.Users[userId];
        }

        public async Task ApplyPreset(string userId, int index)
        {
            var presets = await GetPresets(userId).ConfigureAwait(false);

            if (!presets.RoutingPresets.ContainsKey(index))
            {
                throw new ArgumentException($"index {index} not exists in list of presets");
            }

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
            var currentRoutes = await _routingService.GetCurrentRoutes().ConfigureAwait(false);
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

            await _storageService.SaveJsonMetadata(PRESETS, result, 1, _configurationContext).ConfigureAwait(false);
        }

        public async Task RemovePreset(string userId, int index)
        {
            var userPresets = await GetPresets(userId).ConfigureAwait(false);

            if (!userPresets.RoutingPresets.ContainsKey(index))
            {
                throw new ArgumentException($"index {index} not exists in list of presets");
            }

            await _storageService.UpdateConfiguration<PresetsModel>(PRESETS, 1, _configurationContext, (update) =>
            {
                update.Remove(config => config.Users[userId].RoutingPresets[index]);
            }).ConfigureAwait(false);
        }
    }
}
