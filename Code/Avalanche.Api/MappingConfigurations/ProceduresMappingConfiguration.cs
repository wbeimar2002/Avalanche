using AutoMapper;
using Avalanche.Api.ViewModels;
using Ism.SystemState.Models.Procedure;
using System;

namespace Avalanche.Api.MappingConfigurations
{
    public class ProceduresMappingConfiguration : Profile
    {
        public ProceduresMappingConfiguration()
        {
            CreateMap<ProcedureImage, ProcedureImageViewModel>();

            CreateMap<PatientViewModel, Patient>()
                .ConstructUsing(p => new Patient())
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.MRN, opt => opt.MapFrom(src => src.MRN))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
                .ForMember(dest => dest.Sex, opt => opt.MapFrom(src => src.Sex.Id));

            CreateMap<Avalanche.Shared.Domain.Models.Department, Department>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            CreateMap<Avalanche.Shared.Domain.Models.ProcedureType, ProcedureType>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));


            CreateMap<Avalanche.Shared.Domain.Models.Physician, Physician>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName));

            CreateMap<ActiveProcedureState, ActiveProcedureViewModel>()
                .ConstructUsing(m => new ActiveProcedureViewModel())
                .ForMember(
                    dest => dest.Patient,
                    opt => opt.MapFrom((src, dst) => 
                        null != src ? 
                        new PatientViewModel() { 
                            LastName = src.Patient?.LastName,
                            FirstName = src.Patient?.FirstName,
                            DateOfBirth = src.Patient?.DateOfBirth ?? DateTime.MinValue,
                            Department = null != src.Department ? new Shared.Domain.Models.Department { Id = src.Department.Id, IsNew = false, Name = src.Department.Name } : null,
                            Id = src.Patient?.Id,
                            MRN = src.Patient?.MRN,
                            Physician = null != src.Physician ? new Shared.Domain.Models.Physician {  Id= src.Physician.Id, FirstName = src.Physician.FirstName, LastName = src.Physician.LastName } : null,
                            ProcedureType = null != src.ProcedureType ? new Shared.Domain.Models.ProcedureType { Id = src.ProcedureType.Id, DepartmentId = src.Department?.Id, IsNew = false, Name = src.ProcedureType.Name}: null,
                            AccessInformation = null,
                            Sex = null != src.Patient?.Sex ? MappingUtilities.GetSexViewModel(src.Patient.Sex) : null
                        }: null
                    )) 
                .ForMember(
                    dest => dest.LibraryId,
                    opt => opt.MapFrom(src => src.LibraryId))
                .ForMember(
                    dest => dest.RepositoryId,
                    opt => opt.MapFrom(src => src.RepositoryId))
                .ForMember(
                    dest => dest.RequiresUserConfirmation,
                    opt => opt.MapFrom(src => src.RequiresUserConfirmation))
                .ForMember(
                    dest => dest.Images,
                    opt => opt.MapFrom(src => src.Images));
        }
    }
}
