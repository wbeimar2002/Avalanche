using AutoMapper;
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
            CreateMap<Ism.Streaming.Common.Core.WebRtcSourceMessage, Device>()
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
