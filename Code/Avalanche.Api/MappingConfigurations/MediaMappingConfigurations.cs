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
                .ReverseMap();
        }
    }
}
