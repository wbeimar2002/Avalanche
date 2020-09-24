using AutoMapper;
using Avalanche.Api.ViewModels;
using Google.Protobuf.WellKnownTypes;
using Ism.IsmLogCommon.Core;
using System;

namespace Avalanche.Api.MappingConfigurations
{
    public class PieMappingConfigurations : Profile
    {
        public PieMappingConfigurations()
        {
            CreateMap<AccessInfo, Ism.PatientInfoEngine.Common.Core.Protos.AccessInfoMessage>();
            CreateMap<AccessInfo, Ism.Storage.Common.Core.PatientList.Proto.AccessInfoMessage>();

            CreateMap<Ism.Storage.Common.Core.PatientList.Proto.PatientRecordMessage, Shared.Domain.Models.Patient>()
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
                    opt => opt.MapFrom(src => src.Mrn))
                .ForMember(dest =>
                    dest.Sex,
                    opt => opt.MapFrom(src => GetSex(src.Patient.Sex)))
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
                    dest.Department,
                    opt => opt.MapFrom(src => GetSex(src.Department)))
                .ForMember(dest =>
                    dest.Sex,
                    opt => opt.MapFrom(src => GetSex(src.Patient.Sex)))
                .ReverseMap();

              CreateMap<PatientViewModel, Ism.Storage.Common.Core.PatientList.Proto.PatientRecordMessage>()
                .ForMember(dest =>
                    dest.AccessionNumber,
                    opt => opt.MapFrom(src => "Unknown"))
                .ForMember(dest =>
                    dest.Department,
                    opt => opt.MapFrom(src => src.Department.Id))
                .ForMember(dest =>
                    dest.AdmissionStatus,
                    opt => opt.MapFrom(src => new Ism.Storage.Common.Core.PatientList.Proto.AdmissionStatusMessage()))
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
                    opt => opt.MapFrom(src => new Ism.Storage.Common.Core.PatientList.Proto.FixedDateMessage 
                    { 
                        Day = src.DateOfBirth.Day, 
                        Month = src.DateOfBirth.Month, 
                        Year = src.DateOfBirth.Year 
                    })) 
                .ForPath(dest =>
                    dest.PerformingPhysician.UserId,
                    opt => opt.MapFrom(src => src.Physician.Id))
                .ForPath(dest =>
                    dest.PerformingPhysician.FirstName,
                    opt => opt.MapFrom(src => src.Physician.FirstName))
                .ForPath(dest =>
                    dest.PerformingPhysician.LastName,
                    opt => opt.MapFrom(src => src.Physician.LastName))
                .ForMember(dest =>
                    dest.SecondaryPhysicians,
                    opt => opt.Ignore())
                .ForMember(dest =>
                    dest.Properties,
                    opt => opt.Ignore())
                .ReverseMap();

            CreateMap<PatientViewModel, Ism.PatientInfoEngine.Common.Core.Protos.PatientRecordMessage>()
                .ForMember(dest =>
                    dest.AccessionNumber,
                    opt => opt.MapFrom(src => "Unknown"))
                .ForMember(dest =>
                    dest.Department,
                    opt => opt.MapFrom(src => src.Department.Id))
                .ForMember(dest =>
                    dest.AdmissionStatus,
                    opt => opt.MapFrom(src => new Ism.PatientInfoEngine.Common.Core.Protos.AdmissionStatus()))
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
                    opt => opt.MapFrom(src => new Ism.PatientInfoEngine.Common.Core.Protos.FixedDateMessage
                    {
                        Day = src.DateOfBirth.Day,
                        Month = src.DateOfBirth.Month,
                        Year = src.DateOfBirth.Year
                    }))
                .ForPath(dest =>
                    dest.PerformingPhysician.UserId,
                    opt => opt.MapFrom(src => src.Physician.Id))
                .ForPath(dest =>
                    dest.PerformingPhysician.FirstName,
                    opt => opt.MapFrom(src => src.Physician.FirstName))
                .ForPath(dest =>
                    dest.PerformingPhysician.LastName,
                    opt => opt.MapFrom(src => src.Physician.LastName))
                .ForMember(dest =>
                    dest.SecondaryPhysicians,
                    opt => opt.Ignore())
                .ForMember(dest =>
                    dest.Properties,
                    opt => opt.Ignore())
                .ReverseMap();
        }

        private object GetSex(Ism.Storage.Common.Core.PatientList.Proto.SexMessage sex)
        {
            switch (sex)
            {
                case Ism.Storage.Common.Core.PatientList.Proto.SexMessage.M:
                    return "M";
                case Ism.Storage.Common.Core.PatientList.Proto.SexMessage.F:
                    return "F";
                case Ism.Storage.Common.Core.PatientList.Proto.SexMessage.U:
                default:
                    return "U";
            }
        }

        private Ism.Storage.Common.Core.PatientList.Proto.SexMessage GetSex(string sex)
        {
            switch (sex)
            {
                case "F":
                    return Ism.Storage.Common.Core.PatientList.Proto.SexMessage.F;
                case "M":
                    return Ism.Storage.Common.Core.PatientList.Proto.SexMessage.M;
                case "O":
                case "U":
                default:
                    return Ism.Storage.Common.Core.PatientList.Proto.SexMessage.U;
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
