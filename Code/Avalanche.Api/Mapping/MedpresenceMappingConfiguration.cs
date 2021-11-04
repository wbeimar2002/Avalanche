using AutoMapper;
using Avalanche.Api.ViewModels;
using Ism.MP.V1.Protos;
using Ism.SystemState.Models.Medpresence;

namespace Avalanche.Api.Mapping
{
    public class MedpresenceMappingConfiguration : Profile
    {
        public MedpresenceMappingConfiguration()
        {
            _ = CreateMap<MedpresenceState, MedpresenceStateViewModel>();
            _ = CreateMap<ArchiveServiceViewModel, ArchiveSessionRequest>()
                 .ForMember(dest => dest.SessionId, opt => opt.MapFrom(src => src.SessionId))
                 .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                 .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                 .ForMember(dest => dest.Physician, opt => opt.MapFrom(src => src.Physician != null ? new PhysicianMessage
                 {
                     Id = src.Physician.Id,
                     FirstName = src.Physician.FirstName,
                     LastName = src.Physician.LastName
                 } : null))
                 .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.Department != null ? new DepartmentMessage
                 {
                     Id = src.Department.Id,
                     Name = src.Department.Name

                 } : null))
                 .ForMember(dest => dest.ProcedureType, opt => opt.MapFrom(src => src.ProcedureType != null ? new ProcedureTypeMessage
                 {
                     Id = src.ProcedureType.Id,
                     Name = src.ProcedureType.Name
                 } : null));
        }
    }
}
