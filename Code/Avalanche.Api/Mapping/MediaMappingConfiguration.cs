using AutoMapper;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models.Media;

namespace Avalanche.Api.Mapping
{
    public class MediaMappingConfiguration : Profile
    {
        public MediaMappingConfiguration()
        {
            // domain model to avidis proto model
            CreateMap<AliasIndexModel, AvidisDeviceInterface.V1.Protos.AliasIndexMessage>()
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

            // TODO: webrtc models should not have anything in common with routing models
            CreateMap<Ism.Streaming.V1.Protos.WebRtcSourceMessage, VideoDeviceModel>()
                .ForPath(dest =>
                    dest.Sink.Index,
                    opt => opt.MapFrom(src => src.PreviewIndex))
                .ForMember(dest =>
                    dest.Name,
                    opt => opt.MapFrom(src => src.StreamName))
                .ForMember(dest =>
                    dest.IsVisible,
                    opt => opt.MapFrom(src => true))
                .ForMember(dest =>
                    dest.PositionInScreen,
                    opt => opt.MapFrom(src => 0))
                .ForPath(dest =>
                    dest.Sink.Alias,
                    opt => opt.Ignore())
                .ForMember(dest =>
                    dest.Type,
                    opt => opt.MapFrom(src => src.StreamType))
                .ForMember(dest =>
                    dest.Source,
                    opt => opt.Ignore()) 
                .ReverseMap();

            CreateMap<WebRTCMessaggeModel, Ism.Streaming.V1.Protos.HandleMessageRequest>()
                    .ForMember(dest =>
                        dest.SessionId,
                        opt => opt.MapFrom(src => src.SessionId))
                    .ForPath(dest =>
                        dest.Offer.Message,
                        opt => opt.MapFrom(src => src.Message))
                    .ForPath(dest =>
                        dest.Offer.Type,
                        opt => opt.MapFrom(src => src.Type))
                    .ReverseMap();

            CreateMap<Ism.IsmLogCommon.Core.AccessInfo, Ism.Streaming.V1.Protos.AccessInfoMessage>()
                .ForPath(dest =>
                    dest.ApplicationName,
                    opt => opt.MapFrom(src => src.ApplicationName))
                .ForPath(dest =>
                    dest.Details,
                    opt => opt.MapFrom(src => src.Details))
                .ForPath(dest =>
                    dest.Id,
                    opt => opt.MapFrom(src => src.Id))
                .ForPath(dest =>
                    dest.Ip,
                    opt => opt.MapFrom(src => src.Ip))
                .ForPath(dest =>
                    dest.MachineName,
                    opt => opt.MapFrom(src => src.MachineName))
                .ForPath(dest =>
                    dest.UserName,
                    opt => opt.MapFrom(src => src.UserName))
                .ReverseMap();

            CreateMap<WebRTCSessionModel, Ism.Streaming.V1.Protos.InitSessionRequest>()
                .ForPath(dest =>
                    dest.AccessInfo.ApplicationName,
                    opt => opt.Ignore())
                .ForPath(dest =>
                    dest.AccessInfo.Details,
                    opt => opt.Ignore())
                .ForPath(dest =>
                    dest.AccessInfo.Id,
                    opt => opt.Ignore())
                .ForPath(dest =>
                    dest.AccessInfo.Ip,
                    opt => opt.Ignore())
                .ForPath(dest =>
                    dest.AccessInfo.MachineName,
                    opt => opt.Ignore())
                .ForPath(dest =>
                    dest.AccessInfo.UserName,
                    opt => opt.Ignore())
                .ForPath(dest =>
                    dest.ExternalObservedIp,
                    opt => opt.Ignore())
                .ForMember(dest =>
                    dest.StreamId,
                    opt => opt.MapFrom(src => src.Sink.Alias))
                .ForMember(dest =>
                    dest.SessionId,
                    opt => opt.MapFrom(src => src.SessionId))
                .ForPath(dest =>
                    dest.Offer.BypassMaxStreamRestrictions,
                    opt => opt.MapFrom(src => true))
                .ForPath(dest =>
                    dest.Offer.Aor,
                    opt => opt.MapFrom(src => "AOR"))
                .ForPath(dest =>
                    dest.Offer.Message,
                    opt => opt.MapFrom(src => src.Message))
                .ForPath(dest =>
                    dest.Offer.Type,
                    opt => opt.MapFrom(src => src.Type))
                .ReverseMap();

            CreateMap<WebRTCMessaggeModel, Ism.Streaming.V1.Protos.DeInitSessionRequest>()
                .ForMember(dest =>
                    dest.SessionId,
                    opt => opt.MapFrom(src => src.SessionId))
                .ReverseMap();
        }
    }
}
