using AutoMapper;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Google.Protobuf.WellKnownTypes;
using Ism.Common.Core.Configuration.Models;
using Ism.Storage.Core.DataManagement.V1.Protos;
using System;

namespace Avalanche.Api.MappingConfigurations
{
    public class PieMappingConfigurations : Profile
    {
        public PieMappingConfigurations()
        {
            CreateMap<DepartmentMessage, Department>();
            CreateMap<ProcedureTypeMessage, ProcedureType>();

            CreateMap<Department, AddDepartmentRequest>()
                .ForPath(dest =>
                    dest.Department.Name,
                    opt => opt.MapFrom(src => src.Name))
                .ReverseMap();

            CreateMap<ProcedureType, AddProcedureTypeRequest>()
                .ForPath(dest =>
                    dest.ProcedureType.Name,
                    opt => opt.MapFrom(src => src.Name))
                .ForPath(dest =>
                    dest.ProcedureType.Department,
                    opt => opt.MapFrom(src => src.Department))
                .ReverseMap();

            CreateMap<AddDepartmentResponse, Department>()
                .ForMember(dest =>
                    dest.Name,
                    opt => opt.MapFrom(src => src.Department.Name))
                .ReverseMap();

            CreateMap<AddProcedureTypeResponse, ProcedureType>()
                .ForMember(dest =>
                    dest.Name,
                    opt => opt.MapFrom(src => src.ProcedureType.Name))
                .ForMember(dest =>
                    dest.Department,
                    opt => opt.MapFrom(src => src.ProcedureType.Department))
                .ReverseMap();

            CreateMap < Avalanche.Shared.Domain.Models.User, ConfigurationContext>()
                .ForMember(dest =>
                    dest.IdnId,
                    opt => opt.MapFrom(src => src.IdnId))
                .ForMember(dest =>
                    dest.SiteId,
                    opt => opt.MapFrom(src => src.SiteId))
                .ForMember(dest =>
                    dest.SystemId,
                    opt => opt.MapFrom(src => src.SystemId))
                .ForMember(dest =>
                    dest.UserId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest =>
                    dest.DepartmentId,
                    opt => opt.MapFrom(src => src.DepartmentId))
                .ReverseMap();

            CreateMap<PatientDetailsSearchFilterViewModel, Ism.PatientInfoEngine.V1.Protos.SearchRequest>()
               .ForPath(dest =>
                   dest.SearchFields.Accession,
                   opt => opt.MapFrom(src => src.AccessionNumber))
               .ForPath(dest =>
                   dest.SearchFields.Department,
                   opt => opt.MapFrom(src => src.DepartmentName))
               .ForPath(dest =>
                   dest.SearchFields.Keyword,
                   opt => opt.MapFrom(src => string.Empty))
               .ForPath(dest =>
                   dest.SearchFields.MaxDate,
                   opt => opt.MapFrom(src => src.MaxDate == null? null : src.MaxDate.Value.ToTimestamp()))
               .ForPath(dest =>
                   dest.SearchFields.MinDate,
                   opt => opt.MapFrom(src => src.MinDate == null ? null : src.MinDate.Value.ToTimestamp()))
               .ForPath(dest =>
                   dest.SearchFields.MRN,
                   opt => opt.MapFrom(src => src.MRN))
               .ForPath(dest =>
                   dest.SearchFields.RoomName,
                   opt => opt.MapFrom(src => src.RoomName))
               .ForPath(dest =>
                   dest.SearchFields.LastName,
                   opt => opt.MapFrom(src => src.LastName))
               .ForPath(dest =>
                   dest.SearchFields.ProcedureId,
                   opt => opt.MapFrom(src => src.ProcedureId))
               .ForMember(dest =>
                   dest.FirstRecordIndex,
                   opt => opt.MapFrom(src => src.Page))
               .ForMember(dest =>
                   dest.MaxResults,
                   opt => opt.MapFrom(src => src.Limit))
               .ForMember(dest =>
                   dest.SearchCultureName,
                   opt => opt.MapFrom(src => src.CultureName))
               .ForPath(dest =>
                   dest.AccessInfo.ApplicationName,
                   opt => opt.MapFrom(src => src.AccessInformation.ApplicationName))
               .ForPath(dest =>
                   dest.AccessInfo.Details,
                   opt => opt.MapFrom(src => src.AccessInformation.Details))
               .ForPath(dest =>
                   dest.AccessInfo.Id,
                   opt => opt.MapFrom(src => src.AccessInformation.Id))
               .ForPath(dest =>
                   dest.AccessInfo.Ip,
                   opt => opt.MapFrom(src => src.AccessInformation.Ip))
               .ForPath(dest =>
                   dest.AccessInfo.MachineName,
                   opt => opt.MapFrom(src => src.AccessInformation.MachineName))
               .ForPath(dest =>
                   dest.AccessInfo.UserName,
                   opt => opt.MapFrom(src => src.AccessInformation.UserName))
               .ReverseMap();

            CreateMap<PatientKeywordSearchFilterViewModel, Ism.PatientInfoEngine.V1.Protos.SearchRequest>()
                .ForPath(dest =>
                    dest.SearchFields.Accession,
                    opt => opt.MapFrom(src => string.Empty))
                .ForPath(dest =>
                    dest.SearchFields.Department,
                    opt => opt.MapFrom(src => string.Empty))
                .ForPath(dest =>
                    dest.SearchFields.MaxDate,
                    opt => opt.Ignore()) 
                .ForPath(dest =>
                    dest.SearchFields.MinDate,
                    opt => opt.Ignore()) 
                .ForPath(dest =>
                    dest.SearchFields.MRN,
                    opt => opt.MapFrom(src => string.Empty))
                .ForPath(dest =>
                    dest.SearchFields.RoomName,
                    opt => opt.MapFrom(src => string.Empty))
                .ForPath(dest =>
                    dest.SearchFields.LastName,
                    opt => opt.MapFrom(src => string.Empty))
                .ForPath(dest =>
                    dest.SearchFields.Keyword,
                    opt => opt.MapFrom(src => src.Term))
                .ForMember(dest =>
                    dest.FirstRecordIndex,
                    opt => opt.MapFrom(src => src.Page))
                .ForMember(dest =>
                    dest.MaxResults,
                    opt => opt.MapFrom(src => src.Limit))
                .ForMember(dest =>
                    dest.SearchCultureName,
                    opt => opt.MapFrom(src => src.CultureName))
                .ForPath(dest =>
                    dest.AccessInfo.ApplicationName,
                    opt => opt.MapFrom(src => src.AccessInformation.ApplicationName))
                .ForPath(dest =>
                    dest.AccessInfo.Details,
                    opt => opt.MapFrom(src => src.AccessInformation.Details))
                .ForPath(dest =>
                    dest.AccessInfo.Id,
                    opt => opt.MapFrom(src => src.AccessInformation.Id))
                .ForPath(dest =>
                    dest.AccessInfo.Ip,
                    opt => opt.MapFrom(src => src.AccessInformation.Ip))
                .ForPath(dest =>
                    dest.AccessInfo.MachineName,
                    opt => opt.MapFrom(src => src.AccessInformation.MachineName))
                .ForPath(dest =>
                    dest.AccessInfo.UserName,
                    opt => opt.MapFrom(src => src.AccessInformation.UserName))
                .ReverseMap();

            CreateMap<Ism.IsmLogCommon.Core.AccessInfo, AccessInfo>();

            CreateMap<Ism.Storage.Core.PatientList.V1.Protos.AddPatientRecordResponse, Shared.Domain.Models.Patient>()
                .ForMember(dest =>
                    dest.FirstName,
                    opt => opt.MapFrom(src => src.PatientRecord.Patient.FirstName))
                .ForMember(dest =>
                    dest.LastName,
                    opt => opt.MapFrom(src => src.PatientRecord.Patient.LastName))
                .ForMember(dest =>
                    dest.Department,
                    opt => opt.MapFrom(src => src.PatientRecord.Department))
                .ForMember(dest =>
                    dest.Id,
                    opt => opt.MapFrom(src => src.PatientRecord.InternalId))
                .ForMember(dest =>
                    dest.DateOfBirth,
                    opt => opt.MapFrom(src => new DateTime(src.PatientRecord.Patient.Dob.Year, src.PatientRecord.Patient.Dob.Month, src.PatientRecord.Patient.Dob.Day)))
                .ForMember(dest =>
                    dest.MRN,
                    opt => opt.MapFrom(src => src.PatientRecord.Mrn))
                .ForMember(dest =>
                    dest.Sex,
                    opt => opt.MapFrom(src => GetSex(src.PatientRecord.Patient.Sex)))
                .ReverseMap();

            CreateMap<Ism.PatientInfoEngine.V1.Protos.PatientRecordMessage, Shared.Domain.Models.Patient>()
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

            CreateMap<PatientViewModel, Ism.Storage.Core.PatientList.V1.Protos.AddPatientRecordRequest>()
                .ForPath(dest =>
                    dest.AccessInfo.ApplicationName,
                    opt => opt.MapFrom(src => src.AccessInformation.ApplicationName))
                .ForPath(dest =>
                    dest.AccessInfo.Details,
                    opt => opt.MapFrom(src => src.AccessInformation.Details))
                .ForPath(dest =>
                    dest.AccessInfo.Id,
                    opt => opt.MapFrom(src => src.AccessInformation.Id))
                .ForPath(dest =>
                    dest.AccessInfo.Ip,
                    opt => opt.MapFrom(src => src.AccessInformation.Ip))
                .ForPath(dest =>
                    dest.AccessInfo.MachineName,
                    opt => opt.MapFrom(src => src.AccessInformation.MachineName))
                .ForPath(dest =>
                    dest.AccessInfo.UserName,
                    opt => opt.MapFrom(src => src.AccessInformation.UserName))
                .ForPath(dest =>
                    dest.PatientRecord.AccessionNumber,
                    opt => opt.MapFrom(src => "Unknown"))
                .ForPath(dest =>
                    dest.PatientRecord.Department,
                    opt => opt.MapFrom(src => src.Department.Value))
                .ForPath(dest =>
                    dest.PatientRecord.AdmissionStatus,
                    opt => opt.MapFrom(src => new Ism.Storage.Core.PatientList.V1.Protos.AdmissionStatusMessage()))
                .ForPath(dest =>
                    dest.PatientRecord.InternalId,
                    opt => opt.MapFrom(src => 0))
                .ForPath(dest =>
                    dest.PatientRecord.Room,
                    opt => opt.MapFrom(src => "Unknown"))
                .ForPath(dest =>
                    dest.PatientRecord.Scheduled,
                    opt => opt.MapFrom(src => new Timestamp()))
                .ForPath(dest =>
                    dest.PatientRecord.ProcedureType,
                    opt => opt.MapFrom(src => src.ProcedureType.Value))
                .ForPath(dest =>
                    dest.PatientRecord.ProcedureId,
                    opt => opt.MapFrom(src => "Unknown"))
                .ForPath(dest =>
                    dest.PatientRecord.Mrn,
                    opt => opt.MapFrom(src => src.MRN))
                .ForPath(dest =>
                    dest.PatientRecord.Patient.FirstName,
                    opt => opt.MapFrom(src => src.FirstName))
                .ForPath(dest =>
                    dest.PatientRecord.Patient.LastName,
                    opt => opt.MapFrom(src => src.LastName))
                .ForPath(dest =>
                    dest.PatientRecord.Patient.Sex,
                    opt => opt.MapFrom(src => GetSex(src.Sex.Id)))
                .ForPath(dest =>
                    dest.PatientRecord.Patient.Dob,
                    opt => opt.MapFrom(src => new Ism.Storage.Core.PatientList.V1.Protos.FixedDateMessage
                    {
                        Day = src.DateOfBirth.Day,
                        Month = src.DateOfBirth.Month,
                        Year = src.DateOfBirth.Year
                    }))
                .ForPath(dest =>
                    dest.PatientRecord.PerformingPhysician.UserId,
                    opt => opt.MapFrom(src => src.Physician.Id))
                .ForPath(dest =>
                    dest.PatientRecord.PerformingPhysician.FirstName,
                    opt => opt.MapFrom(src => src.Physician.FirstName))
                .ForPath(dest =>
                    dest.PatientRecord.PerformingPhysician.LastName,
                    opt => opt.MapFrom(src => src.Physician.LastName))
                .ForPath(dest =>
                    dest.PatientRecord.SecondaryPhysicians,
                    opt => opt.Ignore())
                .ForPath(dest =>
                    dest.PatientRecord.Properties,
                    opt => opt.Ignore())
                .ReverseMap();

            CreateMap<PatientViewModel, Ism.Storage.Core.PatientList.V1.Protos.UpdatePatientRecordRequest>()
                .ForPath(dest =>
                    dest.AccessInfo.ApplicationName,
                    opt => opt.MapFrom(src => src.AccessInformation.ApplicationName))
                .ForPath(dest =>
                    dest.AccessInfo.Details,
                    opt => opt.MapFrom(src => src.AccessInformation.Details))
                .ForPath(dest =>
                    dest.AccessInfo.Id,
                    opt => opt.MapFrom(src => src.AccessInformation.Id))
                .ForPath(dest =>
                    dest.AccessInfo.Ip,
                    opt => opt.MapFrom(src => src.AccessInformation.Ip))
                .ForPath(dest =>
                    dest.AccessInfo.MachineName,
                    opt => opt.MapFrom(src => src.AccessInformation.MachineName))
                .ForPath(dest =>
                    dest.AccessInfo.UserName,
                    opt => opt.MapFrom(src => src.AccessInformation.UserName))
                .ForPath(dest =>
                    dest.PatientRecord.AccessionNumber,
                    opt => opt.MapFrom(src => "Unknown"))
                .ForPath(dest =>
                    dest.PatientRecord.Department,
                    opt => opt.MapFrom(src => src.Department.Value))
                .ForPath(dest =>
                    dest.PatientRecord.AdmissionStatus,
                    opt => opt.MapFrom(src => new Ism.Storage.Core.PatientList.V1.Protos.AdmissionStatusMessage()))
                .ForPath(dest =>
                    dest.PatientRecord.InternalId,
                    opt => opt.MapFrom(src => 0))
                .ForPath(dest =>
                    dest.PatientRecord.Room,
                    opt => opt.MapFrom(src => "Unknown"))
                .ForPath(dest =>
                    dest.PatientRecord.Scheduled,
                    opt => opt.MapFrom(src => new Timestamp()))
                .ForPath(dest =>
                    dest.PatientRecord.ProcedureType,
                    opt => opt.MapFrom(src => src.ProcedureType.Value))
                .ForPath(dest =>
                    dest.PatientRecord.ProcedureId,
                    opt => opt.MapFrom(src => "Unknown"))
                .ForPath(dest =>
                    dest.PatientRecord.Mrn,
                    opt => opt.MapFrom(src => src.MRN))
                .ForPath(dest =>
                    dest.PatientRecord.Patient.FirstName,
                    opt => opt.MapFrom(src => src.FirstName))
                .ForPath(dest =>
                    dest.PatientRecord.Patient.LastName,
                    opt => opt.MapFrom(src => src.LastName))
                .ForPath(dest =>
                    dest.PatientRecord.Patient.Sex,
                    opt => opt.MapFrom(src => GetSex(src.Sex.Id)))
                .ForPath(dest =>
                    dest.PatientRecord.Patient.Dob,
                    opt => opt.MapFrom(src => new Ism.Storage.Core.PatientList.V1.Protos.FixedDateMessage
                    {
                        Day = src.DateOfBirth.Day,
                        Month = src.DateOfBirth.Month,
                        Year = src.DateOfBirth.Year
                    }))
                .ForPath(dest =>
                    dest.PatientRecord.PerformingPhysician.UserId,
                    opt => opt.MapFrom(src => src.Physician.Id))
                .ForPath(dest =>
                    dest.PatientRecord.PerformingPhysician.FirstName,
                    opt => opt.MapFrom(src => src.Physician.FirstName))
                .ForPath(dest =>
                    dest.PatientRecord.PerformingPhysician.LastName,
                    opt => opt.MapFrom(src => src.Physician.LastName))
                .ForPath(dest =>
                    dest.PatientRecord.SecondaryPhysicians,
                    opt => opt.Ignore())
                .ForPath(dest =>
                    dest.PatientRecord.Properties,
                    opt => opt.Ignore())
                .ReverseMap();

        }

        private object GetSex(Ism.Storage.Core.PatientList.V1.Protos.SexMessage sex)
        {
            switch (sex)
            {
                case Ism.Storage.Core.PatientList.V1.Protos.SexMessage.M:
                    return "M";
                case Ism.Storage.Core.PatientList.V1.Protos.SexMessage.F:
                    return "F";
                case Ism.Storage.Core.PatientList.V1.Protos.SexMessage.U:
                default:
                    return "U";
            }
        }

        private Ism.Storage.Core.PatientList.V1.Protos.SexMessage GetSex(string sex)
        {
            switch (sex)
            {
                case "F":
                    return Ism.Storage.Core.PatientList.V1.Protos.SexMessage.F;
                case "M":
                    return Ism.Storage.Core.PatientList.V1.Protos.SexMessage.M;
                case "O":
                case "U":
                default:
                    return Ism.Storage.Core.PatientList.V1.Protos.SexMessage.U;
            }
        }

        private string GetSex(Ism.PatientInfoEngine.V1.Protos.Sex sex)
        {
            switch (sex)
            {
                case Ism.PatientInfoEngine.V1.Protos.Sex.M:
                    return "M";
                case Ism.PatientInfoEngine.V1.Protos.Sex.F:
                    return "F";
                case Ism.PatientInfoEngine.V1.Protos.Sex.U:
                default:
                    return "U";
            }
        }
    }
}
