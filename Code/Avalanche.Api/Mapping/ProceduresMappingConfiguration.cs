using AutoMapper;

using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Enumerations;
using Avalanche.Shared.Domain.Models;
using Google.Protobuf.Collections;
using Ism.Library.V1.Protos;
using Ism.SystemState.Models.Procedure;

using System;
using System.Collections.Generic;
using static Avalanche.Api.Mapping.MappingUtilities;

using AutoFixture;
using System.Linq;

namespace Avalanche.Api.Mapping
{
    public class ProceduresMappingConfiguration : Profile
    {
        public ProceduresMappingConfiguration()
        {
            CreateMap(typeof(IEnumerable<>), typeof(RepeatedField<>)).ConvertUsing(typeof(EnumerableToRepeatedFieldTypeConverter<,>));
            CreateMap(typeof(RepeatedField<>), typeof(List<>)).ConvertUsing(typeof(RepeatedFieldToListTypeConverter<,>));

            CreateMap<VideoReference, VideoReferenceViewModel>();

            CreateMap<ProcedureImage, ProcedureImageViewModel>();
            CreateMap<ProcedureVideo, ProcedureVideoViewModel>();

            CreateMap<NoteMessage, NoteModel>();
            CreateMap<NoteModel, NoteMessage>();

            CreateMap<ProcedureContentType, ContentType>();

            CreateMap<VideoReferenceMessage, VideoPositionReferenceViewModel>()
                .ForMember(dest => dest.OffsetFromVideoStart, opt => opt.MapFrom(src => GetTimeSpan(src.OffsetFromVideoStart)));

            CreateMap<VideoPositionReferenceViewModel, VideoReferenceMessage>()
                .ForMember(dest => dest.OffsetFromVideoStart, opt => opt.MapFrom(src => GetFixedTimeSpan(src.OffsetFromVideoStart)));

            CreateMap<ProcedureSearchFilterViewModel, GetFinishedProceduresRequest>()
                .ForMember(dest => dest.Page, opt => opt.MapFrom(src => src.Page))
                .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => src.Limit))
                .ForMember(dest => dest.IsDescending, opt => opt.MapFrom(src => src.IsDescending))
                .ForMember(dest => dest.ProcedureIndexSortingColumn, opt => opt.MapFrom(src => src.ProcedureIndexSortingColumn))
                .ForMember(dest => dest.Keyword, opt => opt.MapFrom(src => src.Keyword))
                .ForMember(dest => dest.StartCreationTime, opt => opt.MapFrom(src => GetFixedDateTime(src.StartCreationTime)))
                .ForMember(dest => dest.EndCreationTime, opt => opt.MapFrom(src => GetFixedDateTime(src.EndCreationTime)))
                .ForMember(dest => dest.PatientId, opt => opt.MapFrom(src => src.PatientId))
                .ForMember(dest => dest.PatientLastName, opt => opt.MapFrom(src => src.PatientLastName))
                .ForMember(dest => dest.PhysicianFirstName, opt => opt.MapFrom(src => src.PhysicianFirstName))
                .ForMember(dest => dest.PhysicianLastName, opt => opt.MapFrom(src => src.PhysicianLastName))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.DepartmentName))
                .ForMember(dest => dest.ProcedureTypeName, opt => opt.MapFrom(src => src.ProcedureTypeName))
                .ForMember(dest => dest.IsClinical, opt => opt.MapFrom(src => src.IsClinical))
                .ForMember(dest => dest.Sex, opt => opt.MapFrom(src => src.Sex))
                .ForMember(dest => dest.HasPendingEdits, opt => opt.MapFrom(src => src.HasPendingEdits));

            CreateMap<ProcedureImageMessage, ImageContentViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.Label))
                .ForMember(dest => dest.VideoReference, opt => opt.MapFrom(src => src.VideoReference))
                .ForMember(dest => dest.BackgroundVideoReference, opt => opt.MapFrom(src => src.BackgroundVideoReference))
                .ForMember(dest => dest.Thumbnail, opt => opt.MapFrom(src => src.Thumbnail))
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FileName))
                .ForMember(dest => dest.CaptureTimeUtc, opt => opt.MapFrom(src => GetDateTime(src.CaptureTimeUtc)));

            CreateMap<ProcedureVideoMessage, VideoContentViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.Label))
                .ForMember(dest => dest.BackgroundVideoReference, opt => opt.MapFrom(src => src.BackgroundVideoReference))
                .ForMember(dest => dest.Thumbnail, opt => opt.MapFrom(src => src.Thumbnail))
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FileName))
                .ForMember(dest => dest.Length, opt => opt.MapFrom(src => src.Length))
                .ForMember(dest => dest.CaptureTimeUtc, opt => opt.MapFrom(src => GetDateTime(src.CaptureTimeUtc)));

            CreateMap<ImageContentViewModel, ProcedureImageMessage>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.Label))
                .ForMember(dest => dest.VideoReference, opt => opt.MapFrom(src => src.VideoReference))
                .ForMember(dest => dest.BackgroundVideoReference, opt => opt.MapFrom(src => src.BackgroundVideoReference))
                .ForMember(dest => dest.Thumbnail, opt => opt.MapFrom(src => src.Thumbnail))
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FileName))
                .ForMember(dest => dest.Stream, opt => opt.Ignore())
                .ForMember(dest => dest.CaptureTimeUtc, opt => opt.MapFrom(src => GetFixedDateTime(src.CaptureTimeUtc)));

            CreateMap<VideoContentViewModel, ProcedureVideoMessage>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.Label))
                .ForMember(dest => dest.BackgroundVideoReference, opt => opt.MapFrom(src => src.BackgroundVideoReference))
                .ForMember(dest => dest.Thumbnail, opt => opt.MapFrom(src => src.Thumbnail))
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FileName))
                .ForMember(dest => dest.Length, opt => opt.MapFrom(src => src.Length))
                .ForMember(dest => dest.Stream, opt => opt.Ignore())
                .ForMember(dest => dest.CaptureTimeUtc, opt => opt.MapFrom(src => GetFixedDateTime(src.CaptureTimeUtc)));

            CreateMap<ProcedureMessage, ProcedureViewModel>()
                .ForMember(dest => dest.UserNames, opt => opt.MapFrom(src => src.UserNames))
                .ForMember(dest => dest.Videos, opt => opt.MapFrom(src => src.Videos))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images))
                .ForMember(dest => dest.BackgroundVideos, opt => opt.MapFrom(src => src.BackgroundVideos))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
                .ForMember(dest => dest.IsClinical, opt => opt.MapFrom(src => src.IsClinical))
                .ForMember(dest => dest.VideoAutoEditStatus, opt => opt.MapFrom(src => src.VideoAutoEditStatus))
                .ForMember(dest => dest.Accession, opt => opt.MapFrom(src => src.Accession))
                .ForMember(dest => dest.ScopeSerialNumber, opt => opt.MapFrom(src => src.ScopeSerialNumber))
                .ForMember(dest => dest.Diagnosis, opt => opt.MapFrom(src => src.Diagnosis))
                .ForPath(dest => dest.Patient.MRN, opt => opt.MapFrom(src => src.Patient.Mrn))
                .ForPath(dest => dest.Patient.FirstName, opt => opt.MapFrom(src => src.Patient.FirstName))
                .ForPath(dest => dest.Patient.LastName, opt => opt.MapFrom(src => src.Patient.LastName))
                .ForPath(dest => dest.Patient.Sex, opt => opt.MapFrom(src => src.Patient.Sex))
                .ForPath(dest => dest.Patient.DateOfBirth, opt => opt.MapFrom(src => GetDateTime(src.Patient.DateOfBirth)))
                .ForPath(dest => dest.Physician.Id, opt => opt.MapFrom(src => src.Physician.Id))
                .ForPath(dest => dest.Physician.FirstName, opt => opt.MapFrom(src => src.Physician.FirstName))
                .ForPath(dest => dest.Physician.LastName, opt => opt.MapFrom(src => src.Physician.LastName))
                .ForPath(dest => dest.Department.Name, opt => opt.MapFrom(src => src.Department))
                .ForPath(dest => dest.ProcedureType.Name, opt => opt.MapFrom(src => src.ProcedureType))
                .ForMember(dest => dest.ProcedureStartTimeUtc, opt => opt.MapFrom(src => GetDateTime(src.ProcedureStartTimeUtc)))
                .ForMember(dest => dest.Repository, opt => opt.MapFrom(src => src.ProcedureId.RepositoryId))
                .ForMember(dest => dest.LibraryId, opt => opt.MapFrom(src => src.ProcedureId.Id))
                .ForMember(dest => dest.TasksStatus, opt => opt.MapFrom(src => GetTasksToShow(src.Tasks.ToList())));

            CreateMap<ProcedureViewModel, ProcedureMessage>()
                .ForMember(dest => dest.Videos, opt => opt.MapFrom(src => src.Videos))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
                .ForMember(dest => dest.Tasks, opt => opt.Ignore())
                .ForMember(dest => dest.IsClinical, opt => opt.MapFrom(src => src.IsClinical))
                .ForMember(dest => dest.VideoAutoEditStatus, opt => opt.MapFrom(src => src.VideoAutoEditStatus))
                .ForMember(dest => dest.Accession, opt => opt.MapFrom(src => src.Accession))
                .ForMember(dest => dest.ScopeSerialNumber, opt => opt.MapFrom(src => src.ScopeSerialNumber))
                .ForMember(dest => dest.Diagnosis, opt => opt.MapFrom(src => src.Diagnosis))
                .ForMember(dest => dest.ClinicalNotes, opt => opt.MapFrom(src => src.ClinicalNotes))
                .ForPath(dest => dest.Patient.Mrn, opt => opt.MapFrom(src => src.Patient.MRN))
                .ForPath(dest => dest.Patient.FirstName, opt => opt.MapFrom(src => src.Patient.FirstName))
                .ForPath(dest => dest.Patient.LastName, opt => opt.MapFrom(src => src.Patient.LastName))
                .ForPath(dest => dest.Patient.Sex, opt => opt.MapFrom(src => src.Patient.Sex))
                .ForPath(dest => dest.Patient.DateOfBirth, opt => opt.MapFrom(src => GetFixedDateTime(src.Patient.DateOfBirth)))
                .ForPath(dest => dest.Physician.Id, opt => opt.MapFrom(src => src.Physician.Id))
                .ForPath(dest => dest.Physician.FirstName, opt => opt.MapFrom(src => src.Physician.FirstName))
                .ForPath(dest => dest.Physician.LastName, opt => opt.MapFrom(src => src.Physician.LastName))
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.Department == null ? null : src.Department.Name))
                .ForMember(dest => dest.ProcedureType, opt => opt.MapFrom(src => src.ProcedureType == null ? null : src.ProcedureType.Name))
                .ForMember(dest => dest.ProcedureStartTimeUtc, opt => opt.MapFrom(src => GetFixedDateTime(src.ProcedureStartTimeUtc)))
                .ForPath(dest => dest.ProcedureId.RepositoryId, opt => opt.MapFrom(src => src.Repository))
                .ForPath(dest => dest.ProcedureId.Id, opt => opt.MapFrom(src => src.LibraryId))
                .ForMember(dest => dest.ProcedureTimezoneId, opt => opt.Ignore()); // TODO: Temporary Fix Development in progress

            CreateMap<ActiveProcedureState, DiscardActiveProcedureRequest>()
                .ForPath(dest => dest.ProcedureId.Id, opt => opt.MapFrom(src => src.LibraryId))
                .ForPath(dest => dest.ProcedureId.RepositoryId, opt => opt.MapFrom(src => src.RepositoryId))
                .ForMember(dest => dest.AccessInfo, opt => opt.Ignore());

            CreateMap<ActiveProcedureState, CommitActiveProcedureRequest>()
                .ForPath(dest => dest.ProcedureId.Id, opt => opt.MapFrom(src => src.LibraryId))
                .ForPath(dest => dest.ProcedureId.RepositoryId, opt => opt.MapFrom(src => src.RepositoryId))
                .ForMember(dest => dest.AccessInfo, opt => opt.Ignore());

            CreateMap<ActiveProcedureState, ProcedureIdMessage>()
                .ForPath(dest => dest.Id, opt => opt.MapFrom(src => src.LibraryId))
                .ForPath(dest => dest.RepositoryId, opt => opt.MapFrom(src => src.RepositoryId));

            CreateMap<ProcedureIdMessage, ProcedureIdViewModel>()
                .ForPath(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForPath(dest => dest.RepositoryId, opt => opt.MapFrom(src => src.RepositoryId));

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
                .ForMember(dest => dest.PatientId, opt => opt.MapFrom(src => src.MRN))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
                .ForMember(dest => dest.Sex, opt => opt.MapFrom(src => src.Sex.Id));

            CreateMap<DepartmentModel, Department>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            CreateMap<ProcedureTypeModel, ProcedureType>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));


            CreateMap<PhysicianModel, Physician>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName));

            CreateMap<ActiveProcedureState, ActiveProcedureViewModel>()
                .ConstructUsing(m => new ActiveProcedureViewModel())
                .ForMember(
                    dest => dest.Patient,
                    opt => opt.MapFrom((src, dst) =>
                        null != src ?
                        new PatientViewModel(){
                            LastName = src.Patient?.LastName,
                            FirstName = src.Patient?.FirstName,
                            DateOfBirth = src.Patient?.DateOfBirth ?? DateTime.MinValue,
                            Department = null != src.Department ? new Shared.Domain.Models.DepartmentModel { Id = src.Department.Id, Name = src.Department.Name } : null,
                            Id = src.Patient?.Id,
                            MRN = src.Patient?.PatientId,
                            Physician = null != src.Physician ? new Shared.Domain.Models.PhysicianModel {  Id = Convert.ToInt32(src.Physician.Id), FirstName = src.Physician.FirstName, LastName = src.Physician.LastName } : null,
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
                .ForMember(dest => dest.IsRecording, opt => opt.MapFrom(src => src.IsRecording))
                .ForMember(dest => dest.IsBackgroundRecording, opt => opt.MapFrom(src => src.IsBackgroundRecording))
                .ForMember(dest => dest.RecorderState, opt => opt.Ignore())
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images))
                .ForMember(dest => dest.Videos, opt => opt.MapFrom(src => src.Videos))
                .ForMember(dest => dest.BackgroundVideos, opt => opt.MapFrom(src => src.BackgroundVideos));


            CreateMap<Ism.IsmLogCommon.Core.AccessInfo, AccessInfoMessage>()
                .ReverseMap();

            CreateMap<ProcedureIdMessage, ProcedureIdViewModel>()
                .ReverseMap();

            CreateMap<AllocateNewProcedureResponse, ProcedureAllocationViewModel>()
                .ReverseMap();

            CreateMap<RecordingTimelineModel, RecordingTimelineViewModel>();

            CreateMap<ProcedureDownloadRequestViewModel, ProcedureDownloadRequest>()
                .ForPath(dest => dest.ProcedureId.Id, opt => opt.MapFrom(src => src.ProcedureId.Id))
                .ForPath(dest => dest.ProcedureId.RepositoryId, opt => opt.MapFrom(src => src.ProcedureId.RepositoryId))
                .ForMember(dest => dest.ContentItemIds, opt => opt.MapFrom(src => src.ContentItemIds.ToList()))
                .ForMember(dest => dest.RequestId, opt => opt.MapFrom(src => src.RequestId))
                .ForMember(dest => dest.IncludePHI, opt => opt.MapFrom(src => src.IncludePHI));
        }

        private FixedDateTimeMessage GetFixedDateTime(DateTime? dateTime)
        {
            if (dateTime == null)
            {
                return null;
            }
            else
            {
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

        private FixedTimeSpanMessage GetFixedTimeSpan(TimeSpan time) => new FixedTimeSpanMessage()
        {
            Hour = time.Hours,
            Minute = time.Minutes,
            Second = time.Seconds,
        };

        private TimeSpan GetTimeSpan(FixedTimeSpanMessage time) => new TimeSpan(time.Hour, time.Minute, time.Second);

        private FixedDateTimeMessage GetFixedDateTime(DateTime dateTime) => new FixedDateTimeMessage()
        {
            Year = dateTime.Year,
            Month = dateTime.Month,
            Day = dateTime.Day,
            Hour = dateTime.Hour,
            Minute = dateTime.Minute,
            Second = dateTime.Second,
        };

        private FixedDateTimeMessage GetFixedDateTime(DateTimeOffset dateTime) => new FixedDateTimeMessage()
        {
            Year = dateTime.Year,
            Month = dateTime.Month,
            Day = dateTime.Day,
            Hour = dateTime.Hour,
            Minute = dateTime.Minute,
            Second = dateTime.Second,
        };

        private DateTime GetDateTime(FixedDateTimeMessage dateTime) => new DateTime(dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                dateTime.Hour,
                dateTime.Minute,
                dateTime.Second);

        private List<TaskViewModel> GetTasksToShow(List<TaskMessage> tasks)
        {
            var tasksStatus = new List<TaskViewModel>();

            foreach (var item in tasks)
            {
                tasksStatus.Add(new TaskViewModel()
                {
                    Created = GetDateTime(item.Created),
                    Ended = GetDateTime(item.Ended),
                    Status = (TaskStatuses)item.Status,
                    Type = (TaskTypes)item.Type
                })
                ;
            }

            var tasksToShow = tasksStatus
                .OrderByDescending(t => t.Ended)
                .GroupBy(t => t.Type)
                .Select(t => t.FirstOrDefault())
                .ToList();

            return tasksToShow;
        }
    }
}
