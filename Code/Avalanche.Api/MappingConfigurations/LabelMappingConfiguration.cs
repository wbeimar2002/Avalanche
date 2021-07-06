using AutoMapper;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Google.Protobuf.WellKnownTypes;
using Ism.Common.Core.Configuration.Models;
using Ism.Storage.DataManagement.Client.V1.Protos;
using System;

namespace Avalanche.Api.MappingConfigurations
{
    public class LabelMappingConfiguration : Profile
    {
        public LabelMappingConfiguration()
        {
            CreateMap<LabelModel, AddLabelRequest>()
                .ForPath(dest =>
                    dest.Label.Id,
                    opt => opt.MapFrom(src => 0))
                .ForPath(dest =>
                    dest.Label.Name,
                    opt => opt.MapFrom(src => src.Name))
                .ForPath(dest =>
                    dest.Label.ProcedureTypeId,
                    opt => opt.MapFrom(src => src.ProcedureTypeId))
                .ReverseMap();

            CreateMap<AddLabelResponse, LabelModel>()
                .ForMember(dest =>
                    dest.Id,
                    opt => opt.MapFrom(src => src.Label.Id))
                .ForMember(dest =>
                    dest.Name,
                    opt => opt.MapFrom(src => src.Label.Name))
                .ForMember(dest =>
                    dest.ProcedureTypeId,
                    opt => opt.MapFrom(src => src.Label.ProcedureTypeId))
                .ReverseMap();

            CreateMap<LabelModel, DeleteLabelRequest>()
                .ForPath(dest =>
                    dest.LabelId,
                    opt => opt.MapFrom(src => 0))
                .ForPath(dest =>
                    dest.ProcedureTypeId,
                    opt => opt.MapFrom(src => src.ProcedureTypeId))
                .ReverseMap();

            CreateMap<LabelModel, GetLabelsByProcedureTypeRequest>()
                .ForPath(dest =>
                    dest.ProcedureTypeId,
                    opt => opt.MapFrom(src => src.ProcedureTypeId))
                .ReverseMap();

            CreateMap<LabelMessage, LabelModel>()
                .ForMember(dest =>
                    dest.Id,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest =>
                    dest.Name,
                    opt => opt.MapFrom(src => src.Name))
                .ForMember(dest =>
                    dest.ProcedureTypeId,
                    opt => opt.MapFrom(src => src.ProcedureTypeId))
                .ReverseMap();
        }
    }
}
