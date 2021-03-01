using AutoMapper;
using Avalanche.Shared.Domain.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.MappingConfigurations
{
    public class RoutingMappingConfiguration : Profile
    {
        public RoutingMappingConfiguration()
        {
            CreateMap<RegionModel, AvidisDeviceInterface.V1.Protos.ShowPreviewRequest>()
                .ForMember(dest =>
                    dest.PreviewIndex, //TODO: Temporary value
                    opt => opt.MapFrom(src => 0))
                .ForMember(dest =>
                    dest.Height,
                    opt => opt.MapFrom(src => src.Height))
                .ForMember(dest =>
                    dest.Width,
                    opt => opt.MapFrom(src => src.Width))
                .ForMember(dest =>
                    dest.X,
                    opt => opt.MapFrom(src => src.X))
                .ForMember(dest =>
                    dest.Y,
                    opt => opt.MapFrom(src => src.Y))
                .ReverseMap();

            CreateMap<VideoDeviceModel, VideoSinkModel>()
                .ForMember(dest =>
                    dest.Sink,
                    opt => opt.MapFrom(src => src.Sink))
                .ReverseMap();

            CreateMap<VideoDeviceModel, VideoSourceModel>()
                .ForMember(dest =>
                    dest.Sink,
                    opt => opt.MapFrom(src => src.Sink))
                .ForMember(dest =>
                    dest.HasVideo,
                    opt => opt.Ignore())
                .ForMember(dest =>
                    dest.IsDynamic,
                    opt => opt.Ignore())
                .ReverseMap();

            CreateMap<VideoDeviceModel, Ism.Routing.V1.Protos.AliasIndexMessage>()
                .ForMember(dest =>
                    dest.Alias,
                    opt => opt.MapFrom(src => src.Sink.Alias))
                .ForMember(dest =>
                    dest.Index,
                    opt => opt.MapFrom(src => src.Sink.Index))
                .ReverseMap();

            CreateMap<VideoSourceModel, Ism.Routing.V1.Protos.AliasIndexMessage>()
                .ForMember(dest =>
                    dest.Alias,
                    opt => opt.MapFrom(src => src.Sink.Alias))
                .ForMember(dest =>
                    dest.Index,
                    opt => opt.MapFrom(src => src.Sink.Index))
                .ReverseMap();

            CreateMap<VideoSinkModel, Ism.Routing.V1.Protos.AliasIndexMessage>()
                .ForMember(dest =>
                    dest.Alias,
                    opt => opt.MapFrom(src => src.Sink.Alias))
                .ForMember(dest =>
                    dest.Index,
                    opt => opt.MapFrom(src => src.Sink.Index))
                .ReverseMap();

            CreateMap<VideoSourceModel, AvidisDeviceInterface.V1.Protos.AliasIndexMessage>()
                .ForMember(dest =>
                    dest.Alias,
                    opt => opt.MapFrom(src => src.Sink.Alias))
                .ForMember(dest =>
                    dest.Index,
                    opt => opt.MapFrom(src => src.Sink.Index))
                .ReverseMap();

            CreateMap<VideoDeviceModel, AvidisDeviceInterface.V1.Protos.AliasIndexMessage>()
                .ForMember(dest =>
                    dest.Alias,
                    opt => opt.MapFrom(src => src.Sink.Alias))
                .ForMember(dest =>
                    dest.Index,
                    opt => opt.MapFrom(src => src.Sink.Index))
                .ReverseMap();

            CreateMap<Ism.Routing.V1.Protos.VideoSourceMessage, VideoSourceModel>()
                .ForMember(dest =>
                    dest.HasVideo,
                    opt => opt.Ignore())
                .ForPath(dest =>
                    dest.Sink.Alias,
                    opt => opt.MapFrom(src => src.Source.Alias))
                .ForPath(dest =>
                    dest.Sink.Index,
                    opt => opt.MapFrom(src => src.Source.Index))
                .ForMember(dest =>
                    dest.Name,
                    opt => opt.MapFrom(src => src.FriendlyName))
                .ForMember(dest =>
                    dest.PositionInScreen,
                    opt => opt.MapFrom(src => src.ButtonIndex))
                .ForMember(dest =>
                    dest.IsVisible,
                    opt => opt.MapFrom(src => src.ShowInUi))
                .ForMember(dest =>
                    dest.IsDynamic,
                    opt => opt.MapFrom(src => src.IsDynamic))
                .ForMember(dest =>
                    dest.Type,
                    opt => opt.MapFrom(src => src.VideoSourceType))
                .ReverseMap();

            CreateMap<Ism.Routing.V1.Protos.VideoSinkMessage, VideoSinkModel>()
                .ForPath(dest =>
                    dest.Sink.Alias,
                    opt => opt.MapFrom(src => src.Sink.Alias))
                .ForPath(dest =>
                    dest.Sink.Index,
                    opt => opt.MapFrom(src => src.Sink.Index))
                .ForMember(dest =>
                    dest.Name,
                    opt => opt.MapFrom(src => src.FriendlyName))
                .ForMember(dest =>
                    dest.PositionInScreen,
                    opt => opt.MapFrom(src => src.ButtonIndex))
                .ForMember(dest =>
                    dest.Type,
                    opt => opt.MapFrom(src => src.VideoSinkType))
                .ForMember(dest =>
                    dest.IsVisible,
                    opt => opt.MapFrom(src => src.ShowInUi))
                .ReverseMap();
        }
    }
}
