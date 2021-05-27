using AutoMapper;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Utilities;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Domain.Models.Media;
using Avalanche.Shared.Domain.Models.Presets;
using Ism.Common.Core.Configuration.Models;
using Ism.Routing.V1.Protos;
using Ism.Utility.Core;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Ism.Utility.Core.Preconditions;

namespace Avalanche.Api.Managers.Presets
{
    public class PresetManager : IPresetManager
    {
        private readonly IMapper _mapper;
        private readonly IRoutingService _routingService;
        private readonly IStorageService _storageService;
        private readonly UserModel _user;
        private readonly ConfigurationContext _configurationContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string PRESETS = "presets";

        public PresetManager(IRoutingService routingService, IStorageService storageService, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _mapper = ThrowIfNullOrReturn(nameof(mapper), mapper);
            _routingService = ThrowIfNullOrReturn(nameof(routingService), routingService);
            _storageService = ThrowIfNullOrReturn(nameof(storageService), storageService);
            _httpContextAccessor = ThrowIfNullOrReturn(nameof(httpContextAccessor), httpContextAccessor);

            _user = HttpContextUtilities.GetUser(_httpContextAccessor.HttpContext);
            _configurationContext = _mapper.Map<UserModel, ConfigurationContext>(_user);
            _configurationContext.IdnId = Guid.NewGuid().ToString();
        }

        public async Task<UserPresetsModel> GetPresets()
        {
            var userPresets = await _storageService.GetJsonObject<PresetsModel>(PRESETS, 1, _configurationContext);
            if (!userPresets.Users.ContainsKey(_user.Id))
                throw new ArgumentException($"user presets for {_user.Id} does not exists");

            return userPresets.Users[_user.Id];
        }

        public async Task ApplyPreset(int index)
        {
            var presets = await GetPresets();

            if (!presets.RoutingPresets.ContainsKey(index))
                throw new ArgumentException($"index {index} not exists in list of presets");

            var routingPresetModel = presets.RoutingPresets[index];

            // Route videos as per routing presets
            foreach (var route in routingPresetModel.Routes)
            {
                await _routingService.RouteVideo(new RouteVideoRequest()
                {
                    Sink = _mapper.Map<AliasIndexModel, AliasIndexMessage>(route.Sink),
                    Source = _mapper.Map<AliasIndexModel, AliasIndexMessage>(route.Source),
                });
            }
        }        

        public async Task SavePreset(int index, string name)
        {
            // Verify user is valid 
            Preconditions.ThrowIfNullOrEmptyOrWhiteSpace(nameof(_user.Id), _user.Id);


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
            presetsModel.Users[_user.Id] = userPresetModel;

            var result = JsonConvert.SerializeObject(presetsModel);

            await _storageService.SaveJsonObject(PRESETS, result, 1, _configurationContext);
        }
    }
}
