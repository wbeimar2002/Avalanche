using AutoMapper;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Ism.Routing.Common.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.MappingConfigurations
{
    public class VideoRotingMappingConfigurations : Profile
    {
        public VideoRotingMappingConfigurations()
        {
            CreateMap<Device, Source>();
            CreateMap<Device, Output>();

            CreateMap<Device, AliasIndexMessage>()
                .ForMember(dest =>
                    dest.Alias,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest =>
                    dest.Index,
                    opt => opt.MapFrom(src => src.InternalIndex))
                .ReverseMap();

            CreateMap<Source, AliasIndexMessage>()
                .ForMember(dest =>
                    dest.Alias,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest =>
                    dest.Index,
                    opt => opt.MapFrom(src => src.InternalIndex))
                .ReverseMap();

            CreateMap<Output, AliasIndexMessage>()
                .ForMember(dest =>
                    dest.Alias,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest =>
                    dest.Index,
                    opt => opt.MapFrom(src => src.InternalIndex))
                .ReverseMap();

            CreateMap<Ism.Routing.Common.Core.VideoSourceMessage, Source>()
                //.ForMember(dest =>
                //    dest.Group,
                //    opt => opt.MapFrom(src => GetRandomCharacter("AB", new Random())))
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
                //.ForMember(dest =>
                //    dest.Group,
                //    opt => opt.MapFrom(src => GetRandomCharacter("AB", new Random())))
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

        //TODO: Remove this
        //public char GetRandomCharacter(string text, Random rng)
        //{
        //    int index = rng.Next(text.Length);
        //    return text[index];
        //}
    }
}
