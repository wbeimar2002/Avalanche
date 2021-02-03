using AutoMapper;
using Avalanche.Shared.Domain.Models;

namespace Avalanche.Api.MappingConfigurations
{
    public class RoutingMappingConfigurations : Profile
    {
        public RoutingMappingConfigurations()
        {
            CreateMap<Region, AvidisDeviceInterface.V1.Protos.ShowPreviewRequest>()
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

            CreateMap<Command, AvidisDeviceInterface.V1.Protos.HidePreviewRequest>()
                .ForMember(dest =>
                    dest.PreviewIndex, //TODO: Temporary value
                    opt => opt.MapFrom(src => 0))
                .ReverseMap();

            CreateMap<Command, AvidisDeviceInterface.V1.Protos.RoutePreviewRequest>()
                .ForMember(dest =>
                    dest.PreviewIndex, //TODO: Temporary value
                    opt => opt.MapFrom(src => 0))
                .ForPath(dest =>
                    dest.Source.Alias,
                    opt => opt.MapFrom(src => src.Device.Alias))
                .ForPath(dest =>
                    dest.Source.Index,
                    opt => opt.MapFrom(src => src.Device.Index))
                .ReverseMap();

            CreateMap<VideoDevice, VideoSink>()
                .ForMember(dest =>
                    dest.Alias,
                    opt => opt.MapFrom(src => src.Alias))
                .ForMember(dest =>
                    dest.Index,
                    opt => opt.MapFrom(src => src.Index))
                .ReverseMap();

            CreateMap<VideoDevice, VideoSource>()
                .ForMember(dest =>
                    dest.Alias,
                    opt => opt.MapFrom(src => src.Alias))
                .ForMember(dest =>
                    dest.HasVideo,
                    opt => opt.Ignore())
                .ForMember(dest =>
                    dest.Index,
                    opt => opt.MapFrom(src => src.Index))
                .ForMember(dest =>
                    dest.IsDynamic,
                    opt => opt.Ignore())
                .ReverseMap();

            CreateMap<VideoDevice, Ism.Routing.V1.Protos.AliasIndexMessage>()
                .ForMember(dest =>
                    dest.Alias,
                    opt => opt.MapFrom(src => src.Alias))
                .ForMember(dest =>
                    dest.Index,
                    opt => opt.MapFrom(src => src.Index))
                .ReverseMap();

            CreateMap<VideoSource, Ism.Routing.V1.Protos.AliasIndexMessage>()
                .ForMember(dest =>
                    dest.Alias,
                    opt => opt.MapFrom(src => src.Alias))
                .ForMember(dest =>
                    dest.Index,
                    opt => opt.MapFrom(src => src.Index))
                .ReverseMap();

            CreateMap<VideoSink, Ism.Routing.V1.Protos.AliasIndexMessage>()
                .ForMember(dest =>
                    dest.Alias,
                    opt => opt.MapFrom(src => src.Alias))
                .ForMember(dest =>
                    dest.Index,
                    opt => opt.MapFrom(src => src.Index))
                .ReverseMap();

            CreateMap<VideoSource, AvidisDeviceInterface.V1.Protos.AliasIndexMessage>()
                .ForMember(dest =>
                    dest.Alias,
                    opt => opt.MapFrom(src => src.Alias))
                .ForMember(dest =>
                    dest.Index,
                    opt => opt.MapFrom(src => src.Index))
                .ReverseMap();

            CreateMap<VideoDevice, AvidisDeviceInterface.V1.Protos.AliasIndexMessage>()
                .ForMember(dest =>
                    dest.Alias,
                    opt => opt.MapFrom(src => src.Alias))
                .ForMember(dest =>
                    dest.Index,
                    opt => opt.MapFrom(src => src.Index))
                .ReverseMap();

            CreateMap<Ism.Routing.V1.Protos.VideoSourceMessage, VideoSource>()
                .ForMember(dest =>
                    dest.HasVideo,
                    opt => opt.Ignore())
                .ForMember(dest =>
                    dest.Alias,
                    opt => opt.MapFrom(src => src.Source.Alias))
                .ForMember(dest =>
                    dest.Index,
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

            CreateMap<Ism.Routing.V1.Protos.VideoSinkMessage, VideoSink>()
                .ForMember(dest =>
                    dest.Alias,
                    opt => opt.MapFrom(src => src.Sink.Alias))
                .ForMember(dest =>
                    dest.Index,
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
