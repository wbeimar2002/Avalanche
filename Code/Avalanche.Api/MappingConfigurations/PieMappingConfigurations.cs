using AutoMapper;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Google.Protobuf.WellKnownTypes;
using Ism.IsmLogCommon.Core;
using Ism.Routing.Common.Core;
using Ism.Storage.Common.Core.PatientList.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalanche.Api.MappingConfigurations
{
    public class PieMappingConfigurations : Profile
    {
        public PieMappingConfigurations()
        {
            CreateMap<AccessInfo, Ism.Storage.Common.Core.PatientList.Proto.AccessInfoMessage>();
            CreateMap<AccessInfo, Ism.Storage.Common.Core.PatientList.Proto.AccessInfoMessage>();
            CreateMap<Device, Source>();
            CreateMap<Device, Output>();

            CreateMap<Device, AliasIndexMessage>()
                .ForMember(dest =>
                    dest.Alias,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest =>
                    dest.Index,
                    opt => opt.MapFrom(src => src.InternalIndex))
                .ReverseMap();

            CreateMap<Source, AliasIndexMessage>()
                .ForMember(dest =>
                    dest.Alias,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest =>
                    dest.Index,
                    opt => opt.MapFrom(src => src.InternalIndex))
                .ReverseMap();

            CreateMap<Output, AliasIndexMessage>()
                .ForMember(dest =>
                    dest.Alias,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest =>
                    dest.Index,
                    opt => opt.MapFrom(src => src.InternalIndex))
                .ReverseMap();

            CreateMap<Ism.PatientInfoEngine.Common.Core.Protos.PatientRecordMessage, Shared.Domain.Models.Patient>()
                .ForMember(dest =>
                    dest.FirstName,
                    opt => opt.MapFrom(src => src.Patient.FirstName))
                .ForMember(dest =>
                    dest.LastName,
                    opt => opt.MapFrom(src => src.Patient.LastName))
                .ForMember(dest =>
                    dest.Id,
                    opt => opt.MapFrom(src => src.InternalId))
                .ForMember(dest =>
                    dest.DateOfBirth,
                    opt => opt.MapFrom(src => new DateTime(src.Patient.Dob.Year, src.Patient.Dob.Month, src.Patient.Dob.Day)))
                .ForMember(dest =>
                    dest.MRN,
                    opt => opt.MapFrom(src => src.MRN))
                .ForMember(dest =>
                    dest.Gender,
                    opt => opt.MapFrom(src => GetSex(src.Patient.Sex)))
                .ReverseMap();

              CreateMap<PatientViewModel, Ism.Storage.Common.Core.PatientList.Proto.PatientRecordMessage>()
                .ForMember(dest =>
                    dest.AccessionNumber,
                    opt => opt.MapFrom(src => "Unknown"))
                .ForMember(dest =>
                    dest.Department,
                    opt => opt.MapFrom(src => "Unknown"))
                .ForMember(dest =>
                    dest.AdmissionStatus,
                    opt => opt.MapFrom(src => new AdmissionStatusMessage()))
                .ForMember(dest =>
                    dest.InternalId,
                    opt => opt.MapFrom(src => 0))
                .ForMember(dest =>
                    dest.Room,
                    opt => opt.MapFrom(src => "Unknown"))
                .ForMember(dest =>
                    dest.Scheduled,
                    opt => opt.MapFrom(src => new Timestamp()))
                .ForMember(dest =>
                    dest.ProcedureType,
                    opt => opt.MapFrom(src => src.ProcedureType.Id))
                .ForMember(dest =>
                    dest.ProcedureId,
                    opt => opt.MapFrom(src => "Unknown"))
                .ForPath(dest =>
                    dest.Patient.FirstName,
                    opt => opt.MapFrom(src => src.FirstName))
                .ForPath(dest =>
                    dest.Patient.LastName,
                    opt => opt.MapFrom(src => src.LastName))
                .ForPath(dest =>
                    dest.Patient.Sex,
                    opt => opt.MapFrom(src => GetSex(src.Sex.Id)))
                .ForPath(dest =>
                    dest.Patient.Dob,
                    opt => opt.MapFrom(src => GetFixedDateMessage(src.DateOfBirth))) 
                .ForPath(dest =>
                    dest.PerformingPhysician.UserId,
                    opt => opt.MapFrom(src => src.Physician.Id))
                .ForPath(dest =>
                    dest.PerformingPhysician.FirstName,
                    opt => opt.MapFrom(src => src.Physician.FirstName))
                .ForPath(dest =>
                    dest.PerformingPhysician.LastName,
                    opt => opt.MapFrom(src => src.Physician.LastName))
                .ReverseMap();

            CreateMap<PatientViewModel, Ism.PatientInfoEngine.Common.Core.Protos.PatientRecordMessage>()
                .ForMember(dest =>
                    dest.AccessionNumber,
                    opt => opt.MapFrom(src => "Unknown"))
                .ForMember(dest =>
                    dest.Department,
                    opt => opt.MapFrom(src => "Unknown"))
                .ForMember(dest =>
                    dest.AdmissionStatus,
                    opt => opt.MapFrom(src => new AdmissionStatusMessage()))
                .ForMember(dest =>
                    dest.InternalId,
                    opt => opt.MapFrom(src => 0))
                .ForMember(dest =>
                    dest.Room,
                    opt => opt.MapFrom(src => "Unknown"))
                .ForMember(dest =>
                    dest.Scheduled,
                    opt => opt.MapFrom(src => new Timestamp()))
                .ForMember(dest =>
                    dest.ProcedureType,
                    opt => opt.MapFrom(src => src.ProcedureType.Id))
                //.ForMember(dest =>
                //    dest.ProcedureId,
                //    opt => opt.MapFrom(src => "Unknown"))
                .ForPath(dest =>
                    dest.Patient.FirstName,
                    opt => opt.MapFrom(src => src.FirstName))
                .ForPath(dest =>
                    dest.Patient.LastName,
                    opt => opt.MapFrom(src => src.LastName))
                .ForPath(dest =>
                    dest.Patient.Sex,
                    opt => opt.MapFrom(src => GetSex(src.Sex.Id)))
                .ForPath(dest =>
                    dest.Patient.Dob,
                    opt => opt.MapFrom(src => GetFixedDateMessage(src.DateOfBirth)))
                .ForPath(dest =>
                    dest.PerformingPhysician.UserId,
                    opt => opt.MapFrom(src => src.Physician.Id))
                .ForPath(dest =>
                    dest.PerformingPhysician.FirstName,
                    opt => opt.MapFrom(src => src.Physician.FirstName))
                .ForPath(dest =>
                    dest.PerformingPhysician.LastName,
                    opt => opt.MapFrom(src => src.Physician.LastName))
                .ReverseMap();

        }

        private FixedDateMessage GetFixedDateMessage(DateTime dateOfBirth)
        {
            return new FixedDateMessage { Day = dateOfBirth.Day, Month = dateOfBirth.Month, Year = dateOfBirth.Year };
        }

        private SexMessage GetSex(string gender)
        {
            switch (gender)
            {
                case "F":
                    return SexMessage.F;
                case "M":
                    return SexMessage.M;
                case "O":
                case "U":
                default:
                    return SexMessage.U;
            }
        }

        private string GetSex(Ism.PatientInfoEngine.Common.Core.Protos.Sex sex)
        {
            switch (sex)
            {
                case Ism.PatientInfoEngine.Common.Core.Protos.Sex.M:
                    return "M";
                case Ism.PatientInfoEngine.Common.Core.Protos.Sex.F:
                    return "F";
                case Ism.PatientInfoEngine.Common.Core.Protos.Sex.U:
                default:
                    return "U";
            }
        }
    }
}
