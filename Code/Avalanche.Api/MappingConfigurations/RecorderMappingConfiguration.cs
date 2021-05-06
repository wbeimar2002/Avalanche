﻿using AutoMapper;
using Avalanche.Shared.Domain.Models.Media;
using Ism.Recorder.Core.V1.Protos;

namespace Avalanche.Api.MappingConfigurations
{
    public class RecorderMappingConfiguration : Profile
    {
        public RecorderMappingConfiguration()
        {
            CreateMap<AliasIndexModel, AliasIndexMessage>()
              .ForMember(dest =>
                  dest.Alias,
                  opt => opt.MapFrom(src => src.Alias))
               .ForMember(dest =>
                  dest.Index,
                  opt => opt.MapFrom(src => src.Index))
              .ReverseMap();

            CreateMap<RecordingChannelModel, RecordChannelMessage>()
               .ForPath(dest =>
                   dest.ChannelName,
                   opt => opt.MapFrom(src => src.ChannelName))
                .ForPath(dest =>
                   dest.VideoSink.Alias,
                   opt => opt.MapFrom(src => src.VideoSink.Alias))
                .ForPath(dest =>
                   dest.VideoSink.Index,
                   opt => opt.MapFrom(src => src.VideoSink.Index))
               .ReverseMap();
        }
    }
}