using AutoMapper;

using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Ism.Library.V1.Protos;
using Ism.SystemState.Models.Procedure;

using System;

namespace Avalanche.Api.MappingConfigurations
{
    public class ProceduresMappingConfiguration : Profile
    {
        public ProceduresMappingConfiguration()
        {
            CreateMap<ProcedureImage, ProcedureImageViewModel>();
            CreateMap<ProcedureVideo, ProcedureVideoViewModel>();
            CreateMap<ProcedureContentType, ContentType>();

            CreateMap<ActiveProcedureState, DiscardActiveProcedureRequest>()
                .ForPath(dest => dest.ProcedureId.Id, opt => opt.MapFrom(src => src.LibraryId))
                .ForPath(dest => dest.ProcedureId.RepositoryName, opt => opt.MapFrom(src => src.RepositoryId))
                .ForMember(dest => dest.AccessInfo, opt => opt.Ignore());

            CreateMap<ActiveProcedureState, CommitActiveProcedureRequest>()
                .ForPath(dest => dest.ProcedureId.Id, opt => opt.MapFrom(src => src.LibraryId))
                .ForPath(dest => dest.ProcedureId.RepositoryName, opt => opt.MapFrom(src => src.RepositoryId))
                .ForMember(dest => dest.AccessInfo, opt => opt.Ignore());

            CreateMap<ActiveProcedureState, ProcedureIdMessage>()
                .ForPath(dest => dest.Id, opt => opt.MapFrom(src => src.LibraryId))
                .ForPath(dest => dest.RepositoryName, opt => opt.MapFrom(src => src.RepositoryId));

            CreateMap<Ism.IsmLogCommon.Core.AccessInfo, AccessInfoMessage>()
                .ForPath(dest =>
                    dest.ApplicationName,
                    opt => opt.MapFrom(src => src.ApplicationName))
                .ForPath(dest =>
                    dest.Details,
                    opt => opt.MapFrom(src => src.Details))
                .ForPath(dest =>
                    dest.Id,
                    opt => opt.MapFrom(src => src.Id))
                .ForPath(dest =>
                    dest.Ip,
                    opt => opt.MapFrom(src => src.Ip))
                .ForPath(dest =>
                    dest.MachineName,
                    opt => opt.MapFrom(src => src.MachineName))
                .ForPath(dest =>
                    dest.UserName,
                    opt => opt.MapFrom(src => src.UserName))
                .ReverseMap();

            CreateMap<PatientViewModel, Patient>()
                .ConstructUsing(p => new Patient())
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.MRN, opt => opt.MapFrom(src => src.MRN))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
                .ForMember(dest => dest.Sex, opt => opt.MapFrom(src => src.Sex.Id));

            CreateMap<Avalanche.Shared.Domain.Models.DepartmentModel, Department>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            CreateMap<Avalanche.Shared.Domain.Models.ProcedureTypeModel, ProcedureType>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));


            CreateMap<Avalanche.Shared.Domain.Models.PhysicianModel, Physician>()
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
                            Department = null != src.Department ? new Shared.Domain.Models.DepartmentModel { Id = src.Department.Id, IsNew = false, Name = src.Department.Name } : null,
                            Id = src.Patient?.Id,
                            MRN = src.Patient?.MRN,
                            Physician = null != src.Physician ? new Shared.Domain.Models.PhysicianModel {  Id= src.Physician.Id, FirstName = src.Physician.FirstName, LastName = src.Physician.LastName } : null,
                            ProcedureType = null != src.ProcedureType ? new Shared.Domain.Models.ProcedureTypeModel { Id = src.ProcedureType.Id, DepartmentId = src.Department?.Id, IsNew = false, Name = src.ProcedureType.Name}: null,
                            Sex = null != src.Patient?.Sex ? MappingUtilities.GetSexViewModel(src.Patient.Sex) : null
                        }: null
                    )) 
                .ForMember(dest => dest.LibraryId, opt => opt.MapFrom(src => src.LibraryId))
                .ForMember(dest => dest.RepositoryId, opt => opt.MapFrom(src => src.RepositoryId))
                .ForMember(dest => dest.ProcedureRelativePath, opt => opt.MapFrom(src => src.ProcedureRelativePath))
                .ForMember(dest => dest.ProcedureStartTimeUtc, opt => opt.MapFrom(src => src.ProcedureStartTimeUtc))
                .ForMember(dest => dest.ProcedureTimezoneId, opt => opt.MapFrom(src => src.ProcedureTimezoneId))
                .ForMember(dest => dest.RequiresUserConfirmation, opt => opt.MapFrom(src => src.RequiresUserConfirmation))
                .ForMember(dest => dest.RecorderState, opt => opt.Ignore())
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images))
                .ForMember(dest => dest.Videos, opt => opt.MapFrom(src => src.Videos));


            CreateMap<Ism.IsmLogCommon.Core.AccessInfo, Ism.Library.V1.Protos.AccessInfoMessage>()
                .ReverseMap();

            CreateMap<Ism.Library.V1.Protos.ProcedureIdMessage, ProcedureIdViewModel>()
                .ReverseMap();

            CreateMap<Ism.Library.V1.Protos.AllocateNewProcedureResponse, ProcedureAllocationViewModel>()
                .ReverseMap();


        }
    }
}
