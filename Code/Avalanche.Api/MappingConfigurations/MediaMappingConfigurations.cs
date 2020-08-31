using AutoMapper;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.MappingConfigurations
{
    public class MediaMappingConfigurations : Profile
    {
        public MediaMappingConfigurations()
        {
            CreateMap<Ism.Routing.Common.Core.VideoSourceMessage, Source>()
                .ForMember(dest =>
                    dest.Group,
                    opt => opt.MapFrom(src => GetRandomCharacter("AB", new Random())))
                .ForMember(dest =>
                    dest.Id,
                    opt => opt.MapFrom(src => src.Source.Alias))
                .ForMember(dest =>
                    dest.InternalIndex,
                    opt => opt.MapFrom(src => src.Source.Index))
                .ForMember(dest =>
                    dest.Name,
                    opt => opt.MapFrom(src => src.FriendlyName))
                .ForMember(dest =>
                    dest.PositionInScreen,
                    opt => opt.MapFrom(src => src.ButtonIndex))
                .ForMember(dest =>
                    dest.IsActive,
                    opt => opt.MapFrom(src => src.ShowInUi))
                .ForMember(dest =>
                    dest.IsDynamic,
                    opt => opt.MapFrom(src => src.IsDynamic))
                .ForMember(dest =>
                    dest.Type,
                    opt => opt.MapFrom(src => GetVideoSourceTypeEnum(src.VideoSourceType)))
                .ReverseMap();

            CreateMap<Ism.Routing.Common.Core.VideoSinkMessage, Output>()
                .ForMember(dest =>
                    dest.Group,
                    opt => opt.MapFrom(src => GetRandomCharacter("AB", new Random())))
                .ForMember(dest =>
                    dest.Id,
                    opt => opt.MapFrom(src => src.Sink.Alias))
                .ForMember(dest =>
                    dest.InternalIndex,
                    opt => opt.MapFrom(src => src.Sink.Index))
                .ForMember(dest =>
                    dest.Name,
                    opt => opt.MapFrom(src => src.FriendlyName))
                .ForMember(dest =>
                    dest.PositionInScreen,
                    opt => opt.MapFrom(src => src.ButtonIndex))
                .ForMember(dest =>
                    dest.IsActive,
                    opt => opt.MapFrom(src => src.ShowInUi))
                .ReverseMap();

            //CreateMap<Ism.Routing.Common.Core.VideoRouteMessage, Source>()
            //    .ForMember(dest =>
            //        dest.Output.Id,
            //        opt => opt.MapFrom(src => src.Sink.Alias))
            //    .ForMember(dest =>
            //        dest.Output.InternalIndex,
            //        opt => opt.MapFrom(src => src.Sink.Index))
            //    .ReverseMap();
        }

        private SourceType GetVideoSourceTypeEnum(string videoSourceType)
        {
            switch (videoSourceType)
            {
                case "ptz":
                    return SourceType.Unknown;
                case "strobe":
                    return SourceType.Unknown;
                case "Endo":
                    return SourceType.Endoscope;
                case "Aux":
                    return SourceType.Unknown;
                case "PGS":
                    return SourceType.Unknown;
                case "pc":
                    return SourceType.PC;
                default:
                    return SourceType.Unknown;
            }
        }

        public char GetRandomCharacter(string text, Random rng)
        {
            int index = rng.Next(text.Length);
            return text[index];
        }
    }
}
