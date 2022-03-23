using AutoMapper;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models.Media;
using System.Linq;

namespace Avalanche.Api.Mapping
{
    public class MediaMappingConfiguration : Profile
    {
        public MediaMappingConfiguration()
        {
            CreateMap<AvidisDeviceInterface.V1.Protos.AliasIndexMessage, AliasIndexModel>()
                .ForMember(dest =>
                    dest.Alias,
                    opt => opt.MapFrom(src => src.Alias))
                .ForMember(dest =>
                    dest.Index,
                    opt => opt.MapFrom(src => src.Index))
                .ReverseMap();

            // viewmodel to domain model
            CreateMap<AliasIndexViewModel, AliasIndexModel>()
                .ForMember(dest =>
                    dest.Alias,
                    opt => opt.MapFrom(src => src.Alias))
                .ForMember(dest =>
                    dest.Index,
                    opt => opt.MapFrom(src => src.Index))
                .ReverseMap();

            CreateMap<Avalanche.Api.ViewModels.AliasIndexViewModel, Ism.Routing.V1.Protos.AliasIndexMessage>()
                .ForMember(dest =>
                    dest.Alias,
                    opt => opt.MapFrom(src => src.Alias))
                .ForMember(dest =>
                    dest.Index,
                    opt => opt.MapFrom(src => src.Index))
                .ReverseMap();

            // system state model to domain model
            CreateMap<Ism.SystemState.Models.VideoRouting.AliasIndexModel, AliasIndexModel>()
                .ForMember(dest =>
                    dest.Alias,
                    opt => opt.MapFrom(src => src.Alias))
                .ForMember(dest =>
                    dest.Index,
                    opt => opt.MapFrom(src => src.Index))
                .ReverseMap();

            CreateMap<RouteViewModel, RouteModel>()
                .ForMember(dest =>
                    dest.Source,
                    opt => opt.MapFrom(src => src.Source))
                .ForMember(dest =>
                    dest.Sink,
                    opt => opt.MapFrom(src => src.Sink))
                .ReverseMap();

            // domain model to routing proto model
            CreateMap<AliasIndexModel, Ism.Routing.V1.Protos.AliasIndexMessage>()
               .ForMember(dest =>
                   dest.Alias,
                   opt => opt.MapFrom(src => src.Alias))
                .ForMember(dest =>
                   dest.Index,
                   opt => opt.MapFrom(src => src.Index))
               .ReverseMap();

            CreateMap<VideoDeviceModel, Ism.Routing.V1.Protos.AliasIndexMessage>()
               .ForMember(dest =>
                   dest.Alias,
                   opt => opt.MapFrom(src => src.Sink.Alias))
                .ForMember(dest =>
                   dest.Index,
                   opt => opt.MapFrom(src => src.Sink.Index))
               .ReverseMap();

            CreateMap<Ism.PgsTimeout.V1.Protos.GetPgsVideoFileResponse, GreetingVideoModel>()
               .ForMember(dest =>
                   dest.Index,
                   opt => opt.MapFrom(src => src.VideoFile.Index))
                .ForMember(dest =>
                   dest.Name,
                   opt => opt.MapFrom(src => src.VideoFile.DisplayName))
                .ForMember(dest =>
                   dest.FilePath,
                   opt => opt.MapFrom(src => src.VideoFile.FileName))
               .ReverseMap();

            CreateMap<Ism.PgsTimeout.V1.Protos.PgsVideoFileMessage, GreetingVideoModel>()
               .ForMember(dest =>
                   dest.Index,
                   opt => opt.MapFrom(src => src.Index))
                .ForMember(dest =>
                   dest.Name,
                   opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(dest =>
                   dest.FilePath,
                   opt => opt.MapFrom(src => src.FileName))
               .ReverseMap();

            CreateMap<GreetingVideoModel, Ism.PgsTimeout.V1.Protos.SetPgsVideoFileRequest>()
               .ForPath(dest =>
                   dest.VideoFile.Index,
                   opt => opt.MapFrom(src => src.Index))
                .ForPath(dest =>
                   dest.VideoFile.DisplayName,
                   opt => opt.MapFrom(src => src.Name))
                .ForPath(dest =>
                   dest.VideoFile.FileName,
                   opt => opt.MapFrom(src => src.FilePath))
               .ReverseMap();

            #region webrtc protos

            CreateMap<WebRtcInfo, Ism.Streaming.V1.Protos.WebRtcInfoMessage>()
                    .ForMember(dest =>
                        dest.Aor,
                        opt => opt.MapFrom(src => src.AoR))
                    .ForMember(dest =>
                        dest.BypassMaxStreamRestrictions,
                        opt => opt.MapFrom(src => src.BypassMaxStreamRestrictions))
                    .ForMember(dest =>
                        dest.Message,
                        opt => opt.MapFrom(src => src.Message))
                    .ForMember(dest =>
                        dest.Type,
                        opt => opt.MapFrom(src => src.Type))
                    .ReverseMap();

            CreateMap<HandleWebRtcMessageRequest, Ism.Streaming.V1.Protos.HandleMessageRequest>()
                    .ForMember(dest =>
                        dest.Offer,
                        opt => opt.MapFrom(src => src.Offer))
                    .ForMember(dest =>
                        dest.SessionId,
                        opt => opt.MapFrom(src => src.SessionId))
                    .ReverseMap();

            CreateMap<InitWebRtcSessionRequest, Ism.Streaming.V1.Protos.InitSessionRequest>()
                .ForMember(dest =>
                    dest.ExternalObservedIp,
                    opt => opt.MapFrom(src => src.ExternalIp))
                .ForMember(dest =>
                    dest.Offer,
                    opt => opt.MapFrom(src => src.Offer))
                .ForMember(dest =>
                    dest.RemoteIp,
                    opt => opt.MapFrom(src => src.RemoteIp))
                .ForMember(dest =>
                    dest.RemoteUser,
                    opt => opt.MapFrom(src => src.RemoteUser))
                .ForMember(dest =>
                    dest.SessionId,
                    opt => opt.MapFrom(src => src.SessionId))
                .ForMember(dest =>
                    dest.StreamName,
                    opt => opt.MapFrom(src => src.StreamName))
                .ReverseMap();

            CreateMap<Ism.Streaming.V1.Protos.InitSessionResponse, InitWebRtcSessionResponse>()
                .ForMember(dest =>
                    dest.Answer,
                    opt => opt.MapFrom(src => src.Answer)) // TODO: does this work for repeatedfields?
                .ReverseMap();

            CreateMap<DeInitWebRtcSessionRequest, Ism.Streaming.V1.Protos.DeInitSessionRequest>()
                .ForMember(dest =>
                    dest.SessionId,
                    opt => opt.MapFrom(src => src.SessionId))
                .ReverseMap();

            CreateMap<Ism.Streaming.V1.Protos.GetSourceStreamsResponse, GetWebRtcStreamsResponse>()
                .ForMember(dest =>
                    dest.StreamNames,
                    opt => opt.MapFrom(src => src.StreamNames)) // TODO: does this work for repeatedfields?
                .ReverseMap();

            #endregion webrtc protos

            // domain model to view model
            CreateMap<TileLayoutModel, Ism.Routing.V1.Protos.TileLayoutMessage>()
               .ForMember(dest =>
                   dest.LayoutName,
                   opt => opt.MapFrom(src => src.LayoutName))
               .ForMember(dest =>
                   dest.NumViewports,
                   opt => opt.Ignore())
               .ForMember(dest =>
                   dest.Viewports,
                   opt => opt.MapFrom(src => src.ViewPorts))
               .ReverseMap();

            // domain model to view model
            CreateMap<TileViewportModel, Ism.Routing.V1.Protos.TileViewportMessage>()
               .ForMember(dest =>
                   dest.Layer,
                   opt => opt.MapFrom(src => src.Layer))
               .ForMember(dest =>
                   dest.X,
                   opt => opt.MapFrom(src => src.X))
               .ForMember(dest =>
                   dest.Y,
                   opt => opt.MapFrom(src => src.Y))
               .ForMember(dest =>
                   dest.Width,
                   opt => opt.MapFrom(src => src.Width))
               .ForMember(dest =>
                   dest.Height,
                   opt => opt.MapFrom(src => src.Height))
               .ReverseMap();

            // domain model to view model
            CreateMap<TileVideoRouteModel, Ism.Routing.V1.Protos.TileVideoRouteMessage>()
               .ForMember(dest =>
                   dest.LayoutName,
                   opt => opt.MapFrom(src => src.LayoutName))
               .ForMember(dest =>
                   dest.Sources,
                   opt => opt.MapFrom(src => src.Sources))
               .ForMember(dest =>
                   dest.Sink,
                   opt => opt.MapFrom(src => src.Sink))
               .ForMember(dest =>
                   dest.SourceCount,
                   opt => opt.MapFrom(src => src.SourceCount))
               .ReverseMap();

            // domain model to view model
            CreateMap<RouteVideoTilingModel, Ism.Routing.V1.Protos.RouteVideoTilingRequest>()
               .ForMember(dest =>
                   dest.Sink,
                   opt => opt.MapFrom(src => src.Sink))
               .ForMember(dest =>
                   dest.Source,
                   opt => opt.MapFrom(src => src.Source))
               .ForMember(dest =>
                   dest.ViewportIndex,
                   opt => opt.MapFrom(src => src.ViewportIndex))
               .ReverseMap();
        }
    }
}
