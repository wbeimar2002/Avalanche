using AutoMapper;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.MappingConfigurations
{
    public class MaintenanceMappingConfiguration : Profile
    {
        public MaintenanceMappingConfiguration()
        {
            CreateMap<Department, KeyValuePairViewModel>()
                .ForMember(dest =>
                    dest.Id,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest =>
                    dest.Value,
                    opt => opt.MapFrom(src => src.Name))
                .ReverseMap();
        }
    }
}
