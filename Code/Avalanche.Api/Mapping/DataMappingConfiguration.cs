using AutoMapper;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Google.Protobuf.WellKnownTypes;
using Ism.Common.Core.Configuration.Models;
using Ism.Storage.DataManagement.Client.V1.Protos;
using System;

namespace Avalanche.Api.Mapping
{
    public class DataMappingConfiguration : Profile
    {
        public DataMappingConfiguration()
        {
            CreateMap<ProcedureTypeModel, DeleteProcedureTypeRequest>()
                .ForMember(dest =>
                    dest.DepartmentId,
                    opt => opt.MapFrom(src => src.DepartmentId))
                .ForMember(dest =>
                    dest.ProcedureTypeId,
                    opt => opt.MapFrom(src => src.Id))
                .ReverseMap();

            CreateMap<DepartmentMessage, DepartmentModel>()
                .ForMember(dest =>
                    dest.Id,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest =>
                    dest.Name,
                    opt => opt.MapFrom(src => src.Name))
                .ReverseMap();

            CreateMap<ProcedureTypeMessage, ProcedureTypeModel>()
                .ForMember(dest =>
                    dest.Id,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest =>
                    dest.Name,
                    opt => opt.MapFrom(src => src.Name))
                .ForMember(dest =>
                    dest.DepartmentId,
                    opt => opt.MapFrom(src => src.DepartmentId))
                .ReverseMap();

            CreateMap<DepartmentModel, AddDepartmentRequest>()
                .ForPath(dest =>
                    dest.Department.Id,
                    opt => opt.MapFrom(src => src.Id))
                .ForPath(dest =>
                    dest.Department.Name,
                    opt => opt.MapFrom(src => src.Name))
                .ReverseMap();

            CreateMap<ProcedureTypeModel, AddProcedureTypeRequest>()
                .ForPath(dest =>
                    dest.ProcedureType.Id,
                    opt => opt.MapFrom(src => 0))
                .ForPath(dest =>
                    dest.ProcedureType.Name,
                    opt => opt.MapFrom(src => src.Name))
                .ForPath(dest =>
                    dest.ProcedureType.DepartmentId,
                    opt => opt.MapFrom(src => src.DepartmentId))
                .ReverseMap();

            CreateMap<AddDepartmentResponse, DepartmentModel>()
                .ForMember(dest =>
                    dest.Id,
                    opt => opt.MapFrom(src => src.Department.Id))
                .ForMember(dest =>
                    dest.Name,
                    opt => opt.MapFrom(src => src.Department.Name))
                .ReverseMap();

            CreateMap<AddProcedureTypeResponse, ProcedureTypeModel>()
                .ForMember(dest =>
                    dest.Id,
                    opt => opt.MapFrom(src => src.ProcedureType.Id))
                .ForMember(dest =>
                    dest.Name,
                    opt => opt.MapFrom(src => src.ProcedureType.Name))
                .ForMember(dest =>
                    dest.DepartmentId,
                    opt => opt.MapFrom(src => src.ProcedureType.DepartmentId))
                .ReverseMap();

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

            CreateMap<LabelModel, UpdateLabelRequest>()
                .ForPath(dest =>
                    dest.Label.Id,
                    opt => opt.MapFrom(src => src.Id))
                .ForPath(dest =>
                    dest.Label.Name,
                    opt => opt.MapFrom(src => src.Name))
                .ForPath(dest =>
                    dest.Label.ProcedureTypeId,
                    opt => opt.MapFrom(src => src.ProcedureTypeId))
                .ReverseMap();

            CreateMap<LabelModel, DeleteLabelRequest>()
                .ForPath(dest =>
                    dest.LabelId,
                    opt => opt.MapFrom(src => src.Id))
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
                    dest.IsNew,
                    opt => opt.Ignore())
                .ForMember(dest =>
                    dest.ProcedureTypeId,
                    opt => opt.MapFrom(src => src.ProcedureTypeId))
                .ReverseMap();

        }
    }
}
