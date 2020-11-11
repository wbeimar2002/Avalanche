using AutoMapper;
using Avalanche.Shared.Domain.Models;
using System;

namespace Avalanche.Api.MappingConfigurations
{
    public class MediaMappingConfigurations : Profile
    {

        //
        public MediaMappingConfigurations()
        {
            CreateMap<Command, Ism.Streaming.V1.Protos.InitSessionRequest>()
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
                //.ForMember(dest =>
                //    dest.Quality,
                //    opt => opt.MapFrom(src => RxStreamQualityEnum.RxStreamQualityHdVideo))
                //.ForMember(dest =>
                //    dest.RouteToStreamingEncoder,
                //    opt => opt.MapFrom(src => true))
                .ForMember(dest =>
                    dest.StreamId,
                    opt => opt.MapFrom(src => src.Device.Id))
                .ForMember(dest =>
                    dest.SessionId,
                    opt => opt.MapFrom(src => src.AdditionalInfo))
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

            CreateMap<Command, Ism.Streaming.V1.Protos.HandleMessageRequest>()
                .ForMember(dest =>
                    dest.SessionId,
                    opt => opt.MapFrom(src => src.AdditionalInfo))
                .ForPath(dest =>
                    dest.Offer.Message,
                    opt => opt.MapFrom(src => src.Message))
                .ForPath(dest =>
                    dest.Offer.Type,
                    opt => opt.MapFrom(src => src.Type))
                .ReverseMap();

            CreateMap<Command, Ism.Streaming.V1.Protos.DeInitSessionRequest>()
                .ForMember(dest =>
                    dest.SessionId,
                    opt => opt.MapFrom(src => src.AdditionalInfo))
                .ReverseMap();

            CreateMap<Command, Ism.PgsTimeout.V1.Protos.SetTimeoutPageRequest>()
                .ForMember(dest =>
                    dest.PageNumber,
                    opt => opt.MapFrom(src => Convert.ToInt32(src.Message)))
                .ReverseMap();

            CreateMap<Command, Ism.PgsTimeout.V1.Protos.SetPgsTimeoutModeRequest>()
                .ForMember(dest =>
                    dest.Mode,
                    opt => opt.MapFrom(src => (Ism.PgsTimeout.V1.Protos.PgsTimeoutModeEnum)Convert.ToInt32(src.Message)))
                .ReverseMap();

            CreateMap<Command, Ism.Recorder.Core.V1.Protos.RecordMessage>()
                .ForMember(dest =>
                    dest.LibId,
                    opt => opt.MapFrom(src => "Unknown"))
                .ForMember(dest =>
                    dest.RepositoryId,
                    opt => opt.MapFrom(src => "Unknown"))
                .ReverseMap();

            CreateMap<Ism.Streaming.V1.Protos.WebRtcSourceMessage, Device>()
                .ForMember(dest =>
                    dest.InternalIndex,
                    opt => opt.MapFrom(src => src.PreviewIndex))
                .ForMember(dest =>
                    dest.Name,
                    opt => opt.MapFrom(src => src.StreamName))
                .ForMember(dest =>
                    dest.IsActive,
                    opt => opt.MapFrom(src => true))
                .ForMember(dest =>
                    dest.PositionInScreen,
                    opt => opt.MapFrom(src => 0))
                .ForMember(dest =>
                    dest.Id,
                    opt => opt.MapFrom(src => string.Empty))
                .ForMember(dest =>
                    dest.Type,
                    opt => opt.MapFrom(src => src.StreamType))
                .ReverseMap();
        }
    }
}
