using AutoMapper;
using Avalanche.Api.ViewModels;
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
            CreateMap<Ism.PgsTimeout.V1.Protos.GetPgsVideoFileResponse, GreetingVideo>()
               .ForMember(dest =>
                   dest.Index,
                   opt => opt.MapFrom(src => src.VideoFile.VideoIndex))
                .ForMember(dest =>
                   dest.Name,
                   opt => opt.MapFrom(src => src.VideoFile.Name))
                .ForMember(dest =>
                   dest.FilePath,
                   opt => opt.MapFrom(src => src.VideoFile.FilePath))
               .ReverseMap();

            CreateMap<Ism.PgsTimeout.V1.Protos.PgsVideoFileMessage, GreetingVideo>()
               .ForMember(dest =>
                   dest.Index,
                   opt => opt.MapFrom(src => src.VideoIndex))
                .ForMember(dest =>
                   dest.Name,
                   opt => opt.MapFrom(src => src.Name))
                .ForMember(dest =>
                   dest.FilePath,
                   opt => opt.MapFrom(src => src.FilePath))
               .ReverseMap();

            CreateMap<GreetingVideo, Ism.PgsTimeout.V1.Protos.SetPgsVideoFileRequest>()
               .ForPath(dest =>
                   dest.VideoFile.VideoIndex,
                   opt => opt.MapFrom(src => src.Index))
                .ForPath(dest =>
                   dest.VideoFile.Name,
                   opt => opt.MapFrom(src => src.Name))
                .ForPath(dest =>
                   dest.VideoFile.FilePath,
                   opt => opt.MapFrom(src => src.FilePath))
               .ReverseMap();

            CreateMap<StateViewModel, Ism.PgsTimeout.V1.Protos.SetPgsVolumeRequest>()
               .ForMember(dest =>
                   dest.Volume,
                   opt => opt.MapFrom(src => Convert.ToDouble(src.Value)))
               .ReverseMap();

            CreateMap<StateViewModel, Ism.PgsTimeout.V1.Protos.SetPgsVideoPositionRequest>()
               .ForMember(dest =>
                   dest.Position,
                   opt => opt.MapFrom(src => Convert.ToDouble(src.Value)))
               .ReverseMap();

            CreateMap<StateViewModel, Ism.PgsTimeout.V1.Protos.SetTimeoutPageRequest>()
               .ForMember(dest =>
                   dest.PageNumber,
                   opt => opt.MapFrom(src => Convert.ToInt32(src.Value))) 
               .ReverseMap();

            CreateMap<StateViewModel, Ism.PgsTimeout.V1.Protos.SetPgsTimeoutModeRequest>()
               .ForMember(dest =>
                   dest.Mode,
                   opt => opt.MapFrom(src => src.Value)) //TODO: Covert this value correctly
               .ReverseMap();

            CreateMap<StateViewModel, Ism.PgsTimeout.V1.Protos.SetPgsPlaybackStateRequest>()
               .ForMember(dest =>
                   dest.IsPlaying,
                   opt => opt.MapFrom(src => Convert.ToBoolean(src.Value)))
               .ReverseMap();

            CreateMap<StateViewModel, Ism.PgsTimeout.V1.Protos.SetPgsPlaybackStateRequest>()
               .ForMember(dest =>
                   dest.IsPlaying,
                   opt => opt.MapFrom(src => Convert.ToBoolean(src.Value)))
               .ReverseMap();

            CreateMap<StateViewModel, Ism.PgsTimeout.V1.Protos.SetPgsMuteRequest>()
                .ForMember(dest =>
                    dest.IsMuted,
                    opt => opt.MapFrom(src => Convert.ToBoolean(src.Value)))
                .ReverseMap();

            //TODO: Check this. The responde has more information. Do we need this for something?
            CreateMap<Ism.PgsTimeout.V1.Protos.GetTimeoutPdfPathResponse, StateViewModel>()
                .ForMember(dest =>
                    dest.Value,
                    opt => opt.MapFrom(src => src.PdfPath))
                .ReverseMap();


            CreateMap<Ism.PgsTimeout.V1.Protos.GetTimeoutPageCountResponse, StateViewModel>()
                .ForMember(dest =>
                    dest.Value,
                    opt => opt.MapFrom(src => src.PageCount))
                .ReverseMap();

            CreateMap<Ism.PgsTimeout.V1.Protos.GetTimeoutPageResponse, StateViewModel>()
                .ForMember(dest =>
                    dest.Value,
                    opt => opt.MapFrom(src => src.PageNumber))
                .ReverseMap();

            CreateMap<Ism.PgsTimeout.V1.Protos.GetPgsVolumeResponse, StateViewModel>()
                .ForMember(dest =>
                    dest.Value,
                    opt => opt.MapFrom(src => src.Volume))
                .ReverseMap();

            CreateMap<Ism.PgsTimeout.V1.Protos.GetPgsTimeoutModeResponse, StateViewModel>()
                .ForMember(dest =>
                    dest.Value,
                    opt => opt.MapFrom(src => src.Mode))
                .ReverseMap();

            CreateMap<Ism.PgsTimeout.V1.Protos.GetPgsPlaybackStateResponse, StateViewModel>()
                .ForMember(dest =>
                    dest.Value,
                    opt => opt.MapFrom(src => src.IsPlaying))
                .ReverseMap();

            CreateMap<Ism.PgsTimeout.V1.Protos.GetPgsMuteResponse, StateViewModel>()
                .ForMember(dest =>
                    dest.Value,
                    opt => opt.MapFrom(src => src.IsMuted))
                .ReverseMap();

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
