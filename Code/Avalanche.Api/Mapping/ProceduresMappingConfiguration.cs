﻿using AutoMapper;

using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Google.Protobuf.Collections;
using Ism.Library.V1.Protos;
using Ism.SystemState.Models.Procedure;

using System;
using System.Collections.Generic;
using static Avalanche.Api.Mapping.MappingUtilities;

namespace Avalanche.Api.Mapping
{
    public class ProceduresMappingConfiguration : Profile
    {
        public ProceduresMappingConfiguration()
        {
            CreateMap(typeof(IEnumerable<>), typeof(RepeatedField<>)).ConvertUsing(typeof(EnumerableToRepeatedFieldTypeConverter<,>));
            CreateMap(typeof(RepeatedField<>), typeof(List<>)).ConvertUsing(typeof(RepeatedFieldToListTypeConverter<,>));

            CreateMap<ProcedureImage, ProcedureImageViewModel>();
            CreateMap<ProcedureVideo, ProcedureVideoViewModel>();

            CreateMap<NoteMessage, NoteModel>();
            CreateMap<NoteModel, NoteMessage>();

            CreateMap<DateTime, FixedDateTimeMessage>();
            CreateMap<FixedDateTimeMessage, DateTime>();

            CreateMap<DateTimeOffset, FixedDateTimeMessage>();
            CreateMap<FixedDateTimeMessage, DateTimeOffset>();

            CreateMap<ProcedureContentType, ContentType>();

            CreateMap<ProcedureSearchFilterViewModel, GetFinishedProceduresRequest>()
                .ForMember(dest => dest.Page, opt => opt.MapFrom(src => src.Page))
                .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => src.Limit))
                .ForMember(dest => dest.IsDescending, opt => opt.MapFrom(src => src.IsDescending))
                .ForMember(dest => dest.ProcedureIndexSortingColumn, opt => opt.MapFrom(src => src.ProcedureIndexSortingColumn))
                .ForMember(dest => dest.Keyword, opt => opt.MapFrom(src => src.Keyword))
                .ForMember(dest => dest.StartCreationTime, opt => opt.MapFrom(src => GetFixedDateTime(src.StartCreationTime)))
                .ForMember(dest => dest.EndCreationTime, opt => opt.MapFrom(src => GetFixedDateTime(src.EndCreationTime)))
                .ForMember(dest => dest.PatientId, opt => opt.Ignore())
                .ForMember(dest => dest.PatientLastName, opt => opt.Ignore())
                .ForMember(dest => dest.PhysicianId, opt => opt.Ignore())
                .ForMember(dest => dest.DepartmentName, opt => opt.Ignore())
                .ForMember(dest => dest.ProcedureTypeName, opt => opt.Ignore())
                .ForMember(dest => dest.HasPendingEdits, opt => opt.MapFrom(src => src.HasPendingEdits));

            CreateMap<ProcedureAdvancedSearchFilterViewModel, GetFinishedProceduresRequest>()
                .ForMember(dest => dest.Page, opt => opt.MapFrom(src => src.Page))
                .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => src.Limit))
                .ForMember(dest => dest.IsDescending, opt => opt.MapFrom(src => src.IsDescending))
                .ForMember(dest => dest.ProcedureIndexSortingColumn, opt => opt.MapFrom(src => src.ProcedureIndexSortingColumn))
                .ForMember(dest => dest.Keyword, opt => opt.MapFrom(src => src.Keyword))
                .ForMember(dest => dest.StartCreationTime, opt => opt.MapFrom(src => GetFixedDateTime(src.StartCreationTime)))
                .ForMember(dest => dest.EndCreationTime, opt => opt.MapFrom(src => GetFixedDateTime(src.EndCreationTime)))
                .ForMember(dest => dest.PatientId, opt => opt.MapFrom(src => src.PatientId))
                .ForMember(dest => dest.PatientLastName, opt => opt.MapFrom(src => src.PatientLastName))
                .ForMember(dest => dest.PhysicianId, opt => opt.MapFrom(src => src.PhysicianId))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.DepartmentName))
                .ForMember(dest => dest.ProcedureTypeName, opt => opt.MapFrom(src => src.ProcedureTypeName))
                .ForMember(dest => dest.HasPendingEdits, opt => opt.MapFrom(src => src.HasPendingEdits));

            CreateMap<ProcedureImageMessage, ImageContentViewModel>()
                .ForMember(dest => dest.Thumbnail, opt => opt.MapFrom(src => src.Thumbnail))
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FileName))
                .ForMember(dest => dest.CaptureTimeUtc, opt => opt.MapFrom(src => src.CaptureTimeUtc));

            CreateMap<ProcedureVideoMessage, VideoContentViewModel>()
                .ForMember(dest => dest.Thumbnail, opt => opt.MapFrom(src => src.Thumbnail)) 
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FileName))
                .ForMember(dest => dest.Length, opt => opt.MapFrom(src => src.Length))
                .ForMember(dest => dest.CaptureTimeUtc, opt => opt.MapFrom(src => src.CaptureTimeUtc));

            CreateMap<ImageContentViewModel, ProcedureImageMessage>()
                .ForMember(dest => dest.Thumbnail, opt => opt.MapFrom(src => src.Thumbnail))
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FileName))
                .ForMember(dest => dest.CaptureTimeUtc, opt => opt.MapFrom(src => src.CaptureTimeUtc));

            CreateMap<VideoContentViewModel, ProcedureVideoMessage>()
                .ForMember(dest => dest.Thumbnail, opt => opt.MapFrom(src => src.Thumbnail))
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FileName))
                .ForMember(dest => dest.Length, opt => opt.MapFrom(src => src.Length))
                .ForMember(dest => dest.CaptureTimeUtc, opt => opt.MapFrom(src => src.CaptureTimeUtc));

            CreateMap<ProcedureMessage, ProcedureViewModel>()
                .ForMember(dest => dest.Videos, opt => opt.MapFrom(src => src.Videos))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
                .ForMember(dest => dest.IsClinical, opt => opt.MapFrom(src => src.IsClinical))
                .ForMember(dest => dest.Accession, opt => opt.MapFrom(src => src.Accession))
                .ForMember(dest => dest.ScopeSerialNumber, opt => opt.MapFrom(src => src.ScopeSerialNumber))
                .ForMember(dest => dest.Diagnosis, opt => opt.MapFrom(src => src.Diagnosis))
                .ForPath(dest => dest.Patient.MRN, opt => opt.MapFrom(src => src.Patient.Mrn))
                .ForPath(dest => dest.Patient.FirstName, opt => opt.MapFrom(src => src.Patient.FirstName))
                .ForPath(dest => dest.Patient.LastName, opt => opt.MapFrom(src => src.Patient.LastName))
                .ForPath(dest => dest.Patient.Sex, opt => opt.MapFrom(src => src.Patient.Sex))
                .ForPath(dest => dest.Patient.DateOfBirth, opt => opt.MapFrom(src => src.Patient.DateOfBirth))
                .ForPath(dest => dest.Physician.Id, opt => opt.MapFrom(src => src.Physician.Id))
                .ForPath(dest => dest.Physician.FirstName, opt => opt.MapFrom(src => src.Physician.FirstName))
                .ForPath(dest => dest.Physician.LastName, opt => opt.MapFrom(src => src.Physician.LastName))
                .ForPath(dest => dest.Department.Name, opt => opt.MapFrom(src => src.Department))
                .ForPath(dest => dest.ProcedureType.Name, opt => opt.MapFrom(src => src.ProcedureType))
                .ForMember(dest => dest.ProcedureStartTimeUtc, opt => opt.MapFrom(src => src.ProcedureStartTimeUtc))
                .ForMember(dest => dest.Repository, opt => opt.MapFrom(src => src.Repository)) 
                .ForMember(dest => dest.LibraryId, opt => opt.MapFrom(src => src.LibraryId));

            CreateMap<ProcedureViewModel, ProcedureMessage>()
                .ForMember(dest => dest.Videos, opt => opt.MapFrom(src => src.Videos))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
                .ForMember(dest => dest.IsClinical, opt => opt.MapFrom(src => src.IsClinical))
                .ForMember(dest => dest.Accession, opt => opt.MapFrom(src => src.Accession))
                .ForMember(dest => dest.ScopeSerialNumber, opt => opt.MapFrom(src => src.ScopeSerialNumber))
                .ForMember(dest => dest.Diagnosis, opt => opt.MapFrom(src => src.Diagnosis))
                .ForPath(dest => dest.Patient.Mrn, opt => opt.MapFrom(src => src.Patient.MRN))
                .ForPath(dest => dest.Patient.FirstName, opt => opt.MapFrom(src => src.Patient.FirstName))
                .ForPath(dest => dest.Patient.LastName, opt => opt.MapFrom(src => src.Patient.LastName))
                .ForPath(dest => dest.Patient.Sex, opt => opt.MapFrom(src => src.Patient.Sex))
                .ForPath(dest => dest.Patient.DateOfBirth, opt => opt.MapFrom(src => src.Patient.DateOfBirth))
                .ForPath(dest => dest.Physician.Id, opt => opt.MapFrom(src => src.Physician.Id))
                .ForPath(dest => dest.Physician.FirstName, opt => opt.MapFrom(src => src.Physician.FirstName))
                .ForPath(dest => dest.Physician.LastName, opt => opt.MapFrom(src => src.Physician.LastName))
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.Department == null ? null : src.Department.Name))
                .ForMember(dest => dest.ProcedureType, opt => opt.MapFrom(src => src.ProcedureType == null ? null : src.ProcedureType.Name))
                .ForMember(dest => dest.ProcedureStartTimeUtc, opt => opt.MapFrom(src => src.ProcedureStartTimeUtc))
                .ForMember(dest => dest.Repository, opt => opt.MapFrom(src => src.Repository))
                .ForMember(dest => dest.LibraryId, opt => opt.MapFrom(src => src.LibraryId));

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
                            Department = null != src.Department ? new Shared.Domain.Models.DepartmentModel { Id = src.Department.Id, Name = src.Department.Name } : null,
                            Id = src.Patient?.Id,
                            MRN = src.Patient?.MRN,
                            Physician = null != src.Physician ? new Shared.Domain.Models.PhysicianModel {  Id= src.Physician.Id, FirstName = src.Physician.FirstName, LastName = src.Physician.LastName } : null,
                            ProcedureType = null != src.ProcedureType ? new Shared.Domain.Models.ProcedureTypeModel { Id = src.ProcedureType.Id, DepartmentId = src.Department?.Id, Name = src.ProcedureType.Name}: null,
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

            CreateMap<RecordingTimelineModel, RecordingTimelineViewModel>();
        }

        private FixedDateTimeMessage GetFixedDateTime(DateTime? dateTime)
        {
            if (dateTime == null)
                return null;
            else
                return new FixedDateTimeMessage()
                { 
                    Year = dateTime.Value.Year,
                    Month = dateTime.Value.Month,
                    Day = dateTime.Value.Day,
                    Hour = dateTime.Value.Hour,
                    Minute = dateTime.Value.Minute,
                    Second = dateTime.Value.Second,
                };
        }
    }
}