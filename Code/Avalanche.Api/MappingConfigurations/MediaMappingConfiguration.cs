using AutoMapper;
using Avalanche.Shared.Domain.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.MappingConfigurations
{
    public class MediaMappingConfiguration : Profile
    {
        public MediaMappingConfiguration()
        {
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

            CreateMap<WebRTCSessionModel, Ism.Streaming.V1.Protos.InitSessionRequest>()
                .ForPath(dest =>
                    dest.AccessInfo.ApplicationName,
                    opt => opt.MapFrom(src => src.AccessInformation.ApplicationName))
                .ForPath(dest =>
                    dest.AccessInfo.Details,
                    opt => opt.MapFrom(src => src.AccessInformation.Details))
                .ForPath(dest =>
                    dest.AccessInfo.Id,
                    opt => opt.MapFrom(src => src.AccessInformation.Id))
                .ForPath(dest =>
                    dest.AccessInfo.Ip,
                    opt => opt.MapFrom(src => src.AccessInformation.Ip))
                .ForPath(dest =>
                    dest.AccessInfo.MachineName,
                    opt => opt.MapFrom(src => src.AccessInformation.MachineName))
                .ForPath(dest =>
                    dest.AccessInfo.UserName,
                    opt => opt.MapFrom(src => src.AccessInformation.UserName))
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
                .ForPath(dest =>
                    dest.ExternalObservedIp,
                    opt => opt.MapFrom(src => src.AccessInformation.Ip))
                .ReverseMap();

            CreateMap<WebRTCMessaggeModel, Ism.Streaming.V1.Protos.DeInitSessionRequest>()
                .ForMember(dest =>
                    dest.SessionId,
                    opt => opt.MapFrom(src => src.SessionId))
                .ReverseMap();
        }
    }
}
