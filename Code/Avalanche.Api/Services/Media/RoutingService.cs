﻿using Avalanche.Shared.Infrastructure.Services.Settings;
using Ism.Routing.Client.V1;
using Ism.Security.Grpc.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using static Ism.Utility.Core.Preconditions;
using static Ism.Routing.V1.Protos.Routing;

namespace Avalanche.Api.Services.Media
{
    [ExcludeFromCodeCoverage]
    public class RoutingService : IRoutingService
    {
        readonly IConfigurationService _configurationService;
        RoutingSecureClient RoutingClient { get; set; }

        public RoutingService(IConfigurationService configurationService, IGrpcClientFactory<RoutingClient> grpcClientFactory, ICertificateProvider certificateProvider)
        {
            _configurationService = ThrowIfNullOrReturn(nameof(configurationService), configurationService);
            ThrowIfNull(nameof(grpcClientFactory), grpcClientFactory);
            ThrowIfNull(nameof(certificateProvider), certificateProvider);

            var hostIpAddress = _configurationService.GetEnvironmentVariable("hostIpAddress");
            var mediaServiceGrpcPort = _configurationService.GetEnvironmentVariable("mediaServiceGrpcPort");

            RoutingClient = new RoutingSecureClient(grpcClientFactory, hostIpAddress, mediaServiceGrpcPort, certificateProvider);
        }

        public async Task<Ism.Routing.V1.Protos.GetVideoSourcesResponse> GetVideoSources()
        {
            return await RoutingClient.GetVideoSources();
        }

        public async Task<Ism.Routing.V1.Protos.GetAlternativeVideoSourceResponse> GetAlternativeVideoSource(Ism.Routing.V1.Protos.GetAlternativeVideoSourceRequest request)
        {
            return await RoutingClient.GetAlternativeVideoSource(request);
        }

        public async Task<Ism.Routing.V1.Protos.GetVideoSinksResponse> GetVideoSinks()
        {
            return await RoutingClient.GetVideoSinks();
        }

        public async Task<Ism.Routing.V1.Protos.GetCurrentRoutesResponse> GetCurrentRoutes()
        {
            return await RoutingClient.GetCurrentRoutes();
        }

        public async Task<Ism.Routing.V1.Protos.GetVideoStateForSourceResponse> GetVideoStateForSource(Ism.Routing.V1.Protos.GetVideoStateForSourceRequest getVideoStateForSourceRequest)
        {
            return await RoutingClient.GetVideoStateForSource(getVideoStateForSourceRequest);
        }

        public async Task<Ism.Routing.V1.Protos.GetVideoStateForAllSourcesResponse> GetVideoStateForAllSources()
        {
            return await RoutingClient.GetVideoStateForAllSources();
        }

        public async Task EnterFullScreen(Ism.Routing.V1.Protos.EnterFullScreenRequest enterFullScreenRequest)
        {
            await RoutingClient.EnterFullScreen(enterFullScreenRequest);
        }

        public async Task ExitFullScreen(Ism.Routing.V1.Protos.ExitFullScreenRequest exitFullScreenRequest)
        {
            await RoutingClient.ExitFullScreen(exitFullScreenRequest);
        }

        public async Task<Ism.Routing.V1.Protos.GetRouteForSinkResponse> GetRouteForSink(Ism.Routing.V1.Protos.GetRouteForSinkRequest getRouteForSinkRequest)
        {
            return await RoutingClient.GetRouteForSink(getRouteForSinkRequest);
        }

        public async Task RouteVideo(Ism.Routing.V1.Protos.RouteVideoRequest routeVideoRequest)
        {
            await RoutingClient.RouteVideo(routeVideoRequest);
        }
    }
}
