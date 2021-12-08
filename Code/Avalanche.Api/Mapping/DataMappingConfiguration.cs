using AutoMapper;
using Avalanche.Api.ViewModels;
using Avalanche.Security.Server.Client.V1.Protos;
using Avalanche.Shared.Domain.Models;
using Ism.Storage.DataManagement.Client.V1.Protos;

namespace Avalanche.Api.Mapping
{
    public class DataMappingConfiguration : Profile
    {
        public DataMappingConfiguration()
        {
            CreateMap<UserMessage, PhysicianViewModel>()
                .ForMember(dest =>
                    dest.Id,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest =>
                    dest.FirstName,
                    opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest =>
                    dest.LastName,
                    opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest =>
                    dest.UserName,
                    opt => opt.MapFrom(src => src.UserName))
                .ReverseMap();

            CreateMap<ProcedureTypeModel, DeleteProcedureTypeRequest>()
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
                    dest.DepartmentName,
                    opt => opt.MapFrom(src => src.Name))
                .ReverseMap();

            CreateMap<ProcedureTypeModel, AddProcedureTypeRequest>()
                .ForPath(dest =>
                    dest.ProcedureTypeName,
                    opt => opt.MapFrom(src => src.Name))
                .ForPath(dest =>
                    dest.DepartmentId,
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
                    dest.LabelName,
                    opt => opt.MapFrom(src => src.Name))
                .ForPath(dest =>
                    dest.ProcedureTypeId,
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
