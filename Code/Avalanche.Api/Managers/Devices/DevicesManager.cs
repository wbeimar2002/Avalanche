using AutoMapper;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Maintenance;
using Avalanche.Api.Services.Media;
using Avalanche.Api.Utilities;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Enumerations;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalanche.Api.Extensions;

namespace Avalanche.Api.Managers.Devices
{
    public partial class DevicesManager : IDevicesManager
    {
        readonly IAvidisService _avidisService;
        readonly IRecorderService _recorderService;
        readonly IMediaService _mediaService;
        readonly IRoutingService _routingService;
        readonly ILogger<MediaManager> _appLoggerService;
        readonly IMapper _mapper;
        readonly IAccessInfoFactory _accessInfoFactory;
        readonly IHttpContextAccessor _httpContextAccessor;
        readonly IStorageService _storageService;

        readonly User user;

        public DevicesManager(IMediaService mediaService,
            IRoutingService routingService,
            ILogger<MediaManager> appLoggerService,
            IAvidisService avidisService,
            IRecorderService recorderService,
            IAccessInfoFactory accessInfoFactory,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IStorageService storageService)
        {
            _storageService = storageService;
            _recorderService = recorderService;
            _avidisService = avidisService;
            _mediaService = mediaService;
            _routingService = routingService;
            _appLoggerService = appLoggerService;
            _accessInfoFactory = accessInfoFactory;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;

            user = HttpContextUtilities.GetUser(_httpContextAccessor.HttpContext);
        }

        public async Task<List<CommandResponse>> SendCommand(CommandViewModel command)
        {
            command.User = user;

            Preconditions.ThrowIfCountIsLessThan(nameof(command.Devices), command.Devices, 1);

            List<CommandResponse> responses = new List<CommandResponse>();

            foreach (var item in command.Devices)
            {
                var device = _mapper.Map<VideoDevice, VideoSource>(item);

                CommandResponse response = await ExecuteCommandAsync(command.CommandType, new Command()
                {
                    Device = device,
                    Destinations = command.Destinations,
                    Message = command.Message,
                    AdditionalInfo = command.AdditionalInfo,
                    Type = command.Type,
                    User = command.User
                }, user);

                responses.Add(response);
            }

            return responses;
        }

        private async Task<CommandResponse> ExecuteCommandAsync(CommandTypes commandType, Command command, User user)
        {
            _appLoggerService.LogInformation($"{commandType.GetDescription()} command executed on {command.Device.Id.Alias} device.");

            switch (commandType)
            {
                // TODO: these are actually the webrtc commands
                // they should probably be moved to their own controller/manager
                // leaving as is for now as to not break the existing preview implementation
                #region PGS Commands
                case CommandTypes.PgsPlayVideo:
                    return await InitializeVideo(command, user);
                case CommandTypes.PgsStopVideo:
                    return await StopVideo(command);
                case CommandTypes.PgsHandleMessageForVideo:
                    return await HandleMessageForVideo(command);
                #endregion

                #region Timeout Commands
                case CommandTypes.TimeoutPlayPdfSlides:
                    //TODO: What happens with the Pgs Tab??
                    return await PlayTimeoutSlides(command);
                case CommandTypes.TimeoutStopPdfSlides:
                    //TODO: if stop we can to restart the vide from the beginning or we should continue 
                    //in the state before to start the timeout mode. How to do this?
                    return await StopSlidesAndResumeVideo(command, user);
                case CommandTypes.TimeoutNextPdfSlide:
                    return await GoToNextTimeoutSlide(command);
                case CommandTypes.TimeoutPreviousPdfSlide:
                    return await GoToPreviousTimeoutSlide(command);
                case CommandTypes.TimeoutSetCurrentSlide:
                    return await SetTimeoutCurrentSlide(command);
                case CommandTypes.SetTimeoutMode:
                    return await SetTimeoutMode(command);
                #endregion

                #region Operate Commands
                case CommandTypes.EnterFullScreen:
                    return await EnterFullScreen(command);
                case CommandTypes.ExitFullScreen:
                    return await ExitFullScreen(command);
                case CommandTypes.RouteVideoSource:
                    return await RouteVideoSource(command);
                case CommandTypes.UnrouteVideoSource:
                    return await UnrouteVideoSource(command);
                case CommandTypes.ShowVideoRoutingPreview:
                    return await ShowVideoRoutingPreview(command);
                case CommandTypes.HideVideoRoutingPreview:
                    return await HideVideoRoutingPreview(command);
                #endregion Operate Commands

                #region Recorder Commands
                case CommandTypes.StartRecording:
                    return await StartRecording(command);
                case CommandTypes.StopRecording:
                    return await StopRecording(command);
                case CommandTypes.CaptureImage:
                    return await CaptureImage(command);
                #endregion

                default:
                    throw new NotImplementedException();
            }
        }

        public async Task<IList<VideoSource>> GetRoutingSources()
        {
            // get video sources and their states
            // the state collection only contains the AliasIndex and a bool
            var sources = await _routingService.GetVideoSources();
            var states = await _routingService.GetVideoStateForAllSources();

            var listResult = _mapper.Map<IList<Ism.Routing.V1.Protos.VideoSourceMessage>, IList<VideoSource>>(sources.VideoSources);

            foreach (var source in listResult)
            {
                // need to merge the HasVideo and VideoSource collections
                var state = states.SourceStates.SingleOrDefault(x => x.Source.EqualsVideoDevice(source));
                source.HasVideo = state?.HasVideo ?? false;
            }

            return listResult;
        }

        public async Task<VideoSource> GetAlternativeSource(string alias, int index)
        {
            var source = await _routingService.GetAlternativeVideoSource(new Ism.Routing.V1.Protos.GetAlternativeVideoSourceRequest { Source = new Ism.Routing.V1.Protos.AliasIndexMessage { Alias = alias, Index = index } });
            var hasVideo = await _routingService.GetVideoStateForSource(new Ism.Routing.V1.Protos.GetVideoStateForSourceRequest { Source = new Ism.Routing.V1.Protos.AliasIndexMessage { Alias = alias, Index = index } });

            var mappedSource = _mapper.Map<Ism.Routing.V1.Protos.VideoSourceMessage, VideoSource>(source.Source);

            mappedSource.Id = new AliasIndexApiModel(alias, index);

            // you could plug in an ela that has no video connected to it
            mappedSource.HasVideo = hasVideo.HasVideo;

            return mappedSource;
        }

        public async Task<IList<VideoSink>> GetRoutingSinks()
        {
            var sinks = await _routingService.GetVideoSinks();
            var routes = await _routingService.GetCurrentRoutes();

            var listResult = _mapper.Map<IList<Ism.Routing.V1.Protos.VideoSinkMessage>, IList<VideoSink>>(sinks.VideoSinks);
            foreach (var sink in listResult) 
            {
                var route = routes.Routes.SingleOrDefault(x => x.Sink.EqualsVideoDevice(sink));
                // get the current source
                sink.Source = new AliasIndexApiModel(route.Source.Alias, route.Source.Index);
            }
            return listResult;
        }
    }
}
