using AutoMapper;
using Avalanche.Api.Services.Configuration;
using Avalanche.Api.Services.Media;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Avalanche.Shared.Infrastructure.Extensions;
using Avalanche.Shared.Infrastructure.Helpers;
using Ism.PgsTimeout.Common.Core;
using Ism.Streaming.V1.Protos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.Managers.Devices
{
    public class DevicesManager : IDevicesManager
    {
        readonly IAvidisService _avidisService;
        readonly IRecorderService _recorderService;
        readonly IMediaService _mediaService;
        readonly ISettingsService _settingsService;
        readonly IRoutingService _routingService;
        readonly ILogger<MediaManager> _appLoggerService;
        readonly IMapper _mapper;

        public DevicesManager(IMediaService mediaService,
            ISettingsService settingsService,
            IRoutingService routingService,
            ILogger<MediaManager> appLoggerService,
            IAvidisService avidisService,
            IRecorderService recorderService,
            IMapper mapper)
        {
            _recorderService = recorderService;
            _avidisService = avidisService;
            _mediaService = mediaService;
            _settingsService = settingsService;
            _routingService = routingService;
            _appLoggerService = appLoggerService;
            _mapper = mapper;
        }

        public async Task SendCommandAsync(CommandViewModel command)
        {
            Preconditions.ThrowIfCountIsLessThan(nameof(command.Devices), command.Devices, 1);
            foreach (var item in command.Devices)
            {
                await ExecuteCommandAsync(command.CommandType, new Command()
                {
                    Device = _mapper.Map<Device, Source>(item),
                    Destinations = command.Destinations,
                    Message = command.Message,
                    AdditionalInfo = command.AdditionalInfo,
                    Type = command.Type
                });
            }
        }

        private async Task ExecuteCommandAsync(CommandTypes commandType, Command command)
        {
            _appLoggerService.LogInformation($"{commandType.GetDescription()} command executed on {command.Device.Id} device.");

            switch (commandType)
            {
                #region PGS Commands
                case CommandTypes.TimeoutStopPdfSlides:
                    //TODO: if stop we can to restart the vide from the beginning or we should continue in the state before to start the timeout mode
                    await PlayPgsVideo(command);
                    break;

                case CommandTypes.PgsPlayVideo:
                    Preconditions.ThrowIfNull(nameof(command.Message), command.Message);
                    Preconditions.ThrowIfNull(nameof(command.AdditionalInfo), command.AdditionalInfo);

                    var setModeCommand = new Command()
                    {
                        Device = command.Device,
                        Message = ((int)TimeoutModes.Pgs).ToString()
                    };

                    await _mediaService.SetPgsTimeoutModeAsync(_mapper.Map<Command, SetPgsTimeoutModeRequest>(command));

                    await _mediaService.InitSessionAsync(_mapper.Map<Command, InitSessionRequest>(command));
                    break;

                case CommandTypes.PgsStopVideo:
                    await _mediaService.DeInitSessionAsync(_mapper.Map<Command, DeInitSessionRequest>(command));
                    break;

                case CommandTypes.PgsPlayAudio:
                    await _mediaService.InitSessionAsync(_mapper.Map<Command, InitSessionRequest>(command));
                    break;

                //case CommandTypes.PgsStopAudio:
                //    await _mediaService.DeInitSessionAsync(command);
                //    break;

                case CommandTypes.PgsHandleMessageForVideo:
                    Preconditions.ThrowIfNull(nameof(command.Message), command.Message);
                    await _mediaService.HandleMessageAsync(_mapper.Map<Command, HandleMessageRequest>(command));
                    break;

                #endregion

                #region Timeout Commands
                case CommandTypes.TimeoutPlayPdfSlides:
                    //TODO: What happens with the Pgs Tab??
                    command.Message = ((int)TimeoutModes.Timeout).ToString();
                    var setTimeOutModeCommand = new Command()
                    {
                        Device = command.Device,
                        Message = ((int)TimeoutModes.Timeout).ToString()
                    };

                    await _mediaService.SetPgsTimeoutModeAsync(_mapper.Map<Command, SetPgsTimeoutModeRequest>(command));

                    break;

                case CommandTypes.TimeoutNextPdfSlide:
                    await _mediaService.NextPageAsync(command);
                    break;

                case CommandTypes.TimeoutPreviousPdfSlide:
                    await _mediaService.PreviousPageAsync(command);
                    break;

                case CommandTypes.TimeoutSetCurrentSlide:
                    Preconditions.ThrowIfStringIsNotNumber(nameof(command.Message), command.Message);
                    await _mediaService.SetTimeoutPageAsync(_mapper.Map<Command, SetTimeoutPageRequest>(command));
                    break;
                #endregion

                #region Operate Commands
                case CommandTypes.EnterFullScreen:
                    await EnterFullScreen(command);
                    break;
                case CommandTypes.ExitFullScreen:
                    await ExitFullScreen(command);
                    break;
                case CommandTypes.RouteVideoSource:
                    await RouteVideoSource(command);
                    break;
                case CommandTypes.UnrouteVideoSource:
                    await UnrouteVideoSource(command);
                    break;
                case CommandTypes.ShowVideoRoutingPreview:
                    await ShowVideoRoutingPreview(command);
                    break;
                #endregion Operate Commands

                #region Recorder Commands

                case CommandTypes.StartRecording:
                    await StartRecording(command);
                    break;
                case CommandTypes.StopRecording:
                    await StopRecording(command);
                    break;
                #endregion
            }
        }

        #region Routing

        public async Task<IList<Source>> GetOperationsSources()
        {
            var sources = await _routingService.GetVideoSources();
            var currentRoutes = await _routingService.GetCurrentRoutes();

            IList<Source> listResult = _mapper.Map<IList<Ism.Routing.V1.Protos.VideoSourceMessage>, IList<Source>>(sources.VideoSources);

            foreach (var item in currentRoutes.Routes)
            {
                var source = listResult.Where(s => s.Id.Equals(item.Source.Alias) && s.InternalIndex.Equals(item.Source.Index)).FirstOrDefault();
                if (source != null)
                    source.Output = _mapper.Map<Ism.Routing.V1.Protos.AliasIndexMessage, Output>(item.Sink);
            }

            return listResult;
        }

        public async Task<IList<Output>> GetOperationsOutputs()
        {
            var outputs = await _routingService.GetVideoSinks();
            IList<Output> listResult = _mapper.Map<IList<Ism.Routing.V1.Protos.VideoSinkMessage>, IList<Output>>(outputs.VideoSinks);
            return listResult;
        }

        private async Task RouteVideoSource(Command command)
        {
            Preconditions.ThrowIfCountIsLessThan(nameof(command.Destinations), command.Destinations, 1);
            foreach (var item in command.Destinations)
            {
                await _routingService.RouteVideo(new Ism.Routing.V1.Protos.RouteVideoRequest()
                {
                    Sink = _mapper.Map<Device, Ism.Routing.V1.Protos.AliasIndexMessage>(item),
                    Source = _mapper.Map<Device, Ism.Routing.V1.Protos.AliasIndexMessage>(command.Device),
                }); 
            }
        }

        private async Task UnrouteVideoSource(Command command)
        {
            await _routingService.RouteVideo(new Ism.Routing.V1.Protos.RouteVideoRequest()
            {
                Sink = _mapper.Map<Device, Ism.Routing.V1.Protos.AliasIndexMessage>(command.Device),
                Source = new Ism.Routing.V1.Protos.AliasIndexMessage(),
            });
        }

        private async Task ExitFullScreen(Command command)
        {
            await _routingService.ExitFullScreen(new Ism.Routing.V1.Protos.ExitFullScreenRequest()
            {
                UserInterfaceId = Convert.ToInt32(command.AdditionalInfo)
            });
        }

        private async Task EnterFullScreen(Command command)
        {
            await _routingService.EnterFullScreen(new Ism.Routing.V1.Protos.EnterFullScreenRequest()
            {
                Source = _mapper.Map<Device, Ism.Routing.V1.Protos.AliasIndexMessage>(command.Device),
                UserInterfaceId = Convert.ToInt32(command.AdditionalInfo)
            });
        }

        private async Task ShowVideoRoutingPreview(Command command)
        {
            var config = await _settingsService.GetVideoRoutingSettingsAsync();
            var region = JsonConvert.DeserializeObject<Region>(command.AdditionalInfo);

            if (config.Mode == VideoRoutingModes.Hardware)
            {
                //await _avidisService.SetPreviewRegion(new SetPreviewRegionRequest()
                //{
                //    PreviewIndex = 0, //TODO: Temporary value
                //    Height = region.Height,
                //    Width = region.Width,
                //    X = region.X,
                //    Y = region.Y,
                //});

                await RoutePreview(command);

                //await _avidisService.SetPreviewVisible(new SetPreviewVisibleRequest()
                //{
                //    PreviewIndex = 0, //TODO: Temporary values
                //    Visible = command.Device.IsActive
                //});
            }

            if (config.Mode == VideoRoutingModes.Software)
                await RoutePreview(command);
        }

        private async Task RoutePreview(Command command)
        {
            await _avidisService.RoutePreview(new AvidisDeviceInterface.V1.Protos.RoutePreviewRequest()
            {
                PreviewIndex = 0, //TODO: Temporary value
                Source = _mapper.Map<Device, AvidisDeviceInterface.V1.Protos.AliasIndexMessage>(command.Device),
            });
        }
        #endregion Routing

        #region PGS - Timeout
        private async Task PlayPgsVideo(Command command)
        {
            var alwaysOnSettings = await _settingsService.GetTimeoutSettingsAsync();
            //await SetMode(command.Device, alwaysOnSettings.PgsVideoAlwaysOn ? TimeoutModes.Pgs : TimeoutModes.Idle);
            //TODO: Check set mode call
            if (alwaysOnSettings.PgsVideoAlwaysOn)
                await _mediaService.InitSessionAsync(_mapper.Map<Command, InitSessionRequest>(command));
        }

        public Task<List<Output>> GetPGSOutputs()
        {
            List<Output> outputs = new List<Output>();

            outputs.Add(new Output()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                Name = "MAIN TV 1",
                Thumbnail = "https://www.olympus-oste.eu/media/innovations/images_5/2n_research_and_development/entwicklung_produktinnovationen_intro.jpg"
            });

            outputs.Add(new Output()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = false,
                Name = "MAIN TV 2",
                Thumbnail = "https://www.olympus-oste.eu/media/innovations/images_5/2n_research_and_development/entwicklung_produktinnovationen_intro.jpg"
            });

            outputs.Add(new Output()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                Name = "AUX TV 1",
                Thumbnail = "https://www.olympus-oste.eu/media/innovations/images_5/2n_research_and_development/entwicklung_produktinnovationen_systemintegration_6.jpg"
            });

            outputs.Add(new Output()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = false,
                Name = "MAIN TV 2",
                Thumbnail = "https://www.olympus-oste.eu/media/innovations/images_5/2n_research_and_development/entwicklung_produktinnovationen_systemintegration_6.jpg"
            });

            return Task.FromResult(outputs);
        }

        public Task<List<Output>> GetTimeoutOutputs()
        {
            List<Output> outputs = new List<Output>();
            outputs.Add(new Output()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                Name = "Charting System",
            });

            outputs.Add(new Output()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                Name = "Nurse PC",
            });

            outputs.Add(new Output()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                Name = "Phys. PC",
            });

            return Task.FromResult(outputs);
        }

        #endregion PGS - Timeout

        #region Recording

        private async Task StartRecording(Command command)
        {
            await _recorderService.StartRecording();
        }

        private async Task StopRecording(Command command)
        {
            await _recorderService.StopRecording();
        }

        #endregion
    }
}
