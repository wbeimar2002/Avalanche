using System;
using System.Collections.Generic;
using Avalanche.Api.ViewModels;
using Avalanche.Shared.Domain.Models;
using Bogus;
using Ism.Library.V1.Protos;
using Ism.SystemState.Models.Procedure;

namespace Avalanche.Api.Test
{
    public static class Fakers
    {
        public static Faker<PatientViewModel> GetPatientFaker() =>
            new Faker<PatientViewModel>()
            .CustomInstantiator(f =>
                new PatientViewModel()
                {
                    Id = f.Person.Random.ULong(100000, 999999),
                    MRN = f.Random.AlphaNumeric(5),
                    FirstName = f.Person.FirstName,
                    LastName = f.Person.LastName,
                    DateOfBirth = f.Person.DateOfBirth,
                    AccessionNumber = f.Random.String(64),
                    BackgroundRecordingMode = f.PickRandom<Shared.Infrastructure.Enumerations.BackgroundRecordingMode>(),
                    Sex = new KeyValuePairViewModel()
                    {
                        Id = "U",
                        TranslationKey = "",
                        Value = "Unspecified"
                    },
                    Department = new DepartmentModel()
                    {
                        Id = 1,
                        Name = f.Commerce.Department()
                    },
                    ProcedureType = new ProcedureTypeModel()
                    {
                        Id = 1,
                        DepartmentId = 1,
                        Name = f.Commerce.ProductName()
                    },
                    Physician = new PhysicianModel()
                    {
                        Id = 1,
                        FirstName = f.Person.FirstName,
                        LastName = f.Person.LastName
                    }
                });

        public static Faker<ActiveProcedureState> GetActiveProcedureFaker() =>
            new Faker<ActiveProcedureState>()
            .CustomInstantiator(f =>
                new ActiveProcedureState()
                {
                    Patient = new Patient() { LastName = "name" },
                    Images = new List<ProcedureImage>(),
                    Videos = new List<ProcedureVideo>(),
                    BackgroundVideos = new List<ProcedureVideo>(),
                    LibraryId = "libId",
                    RepositoryId = "repId",
                    ProcedureRelativePath = "path",
                    Department = null,
                    ProcedureType = null,
                    Physician = null,
                    RequiresUserConfirmation = false,
                    ProcedureStartTimeUtc = DateTimeOffset.UtcNow,
                    ProcedureTimezoneId = TimeZoneInfo.Local.Id,
                    IsClinical = false,
                    Notes = new List<ProcedureNote>(),
                    Accession = null,
                    RecordingEvents = new List<VideoRecordingEvent>(),
                    BackgroundRecordingMode = BackgroundRecordingMode.StartImmediately,
                    RegistrationMode = RegistrationMode.Quick,
                    PatientListSource = PatientListSource.Local
                });

        public static Faker<ActiveProcedureState> GetActiveProcedureWithProcedureTypeFaker() =>
            new Faker<ActiveProcedureState>()
            .CustomInstantiator(f =>
                new ActiveProcedureState()
                {
                    Patient = new Patient() { LastName = "name" },
                    Images = new List<ProcedureImage>(),
                    Videos = new List<ProcedureVideo>(),
                    BackgroundVideos = new List<ProcedureVideo>(),
                    LibraryId = "libId",
                    RepositoryId = "repId",
                    ProcedureRelativePath = "path",
                    Department = null,
                    ProcedureType = new ProcedureType() { Id = 1, Name = "TestProceType" },
                    Physician = null,
                    RequiresUserConfirmation = false,
                    ProcedureStartTimeUtc = DateTimeOffset.UtcNow,
                    ProcedureTimezoneId = TimeZoneInfo.Local.Id,
                    IsClinical = false,
                    Notes = new List<ProcedureNote>(),
                    Accession = null,
                    RecordingEvents = new List<VideoRecordingEvent>(),
                    BackgroundRecordingMode = BackgroundRecordingMode.StartImmediately,
                    RegistrationMode = RegistrationMode.Quick,
                    PatientListSource = PatientListSource.Local
                });

        public static Faker<ActiveProcedureState> GetActiveProcedureWithOneImageFaker() =>
            new Faker<ActiveProcedureState>()
            .CustomInstantiator(f =>
                new ActiveProcedureState()
                {
                    Patient = new Patient() { LastName = "name" },
                    Images = new List<ProcedureImage>() { new ProcedureImage(Guid.NewGuid(), "source", "channel", false, "path", "path", null, DateTimeOffset.UtcNow, Guid.NewGuid()) },
                    Videos = new List<ProcedureVideo>(),
                    BackgroundVideos = new List<ProcedureVideo>(),
                    LibraryId = "libId",
                    RepositoryId = "repId",
                    ProcedureRelativePath = "path",
                    Department = null,
                    ProcedureType = null,
                    Physician = null,
                    RequiresUserConfirmation = false,
                    ProcedureStartTimeUtc = DateTimeOffset.UtcNow,
                    ProcedureTimezoneId = TimeZoneInfo.Local.Id,
                    IsClinical = false,
                    Notes = new List<ProcedureNote>(),
                    Accession = null,
                    RecordingEvents = new List<VideoRecordingEvent>(),
                    BackgroundRecordingMode = BackgroundRecordingMode.StartImmediately,
                    RegistrationMode = RegistrationMode.Quick,
                    PatientListSource = PatientListSource.Local
                });

        public static Faker<ActiveProcedureState> GetActiveProcedureWithImagesDifferentTimeFaker() =>
            new Faker<ActiveProcedureState>()
            .CustomInstantiator(f =>
                new ActiveProcedureState()
                {
                    Patient = new Patient() { LastName = "name" },
                    Images = new List<ProcedureImage>
                    {
                        new ProcedureImage(Guid.NewGuid(), "source", "channel", false, "path", "path", null, DateTimeOffset.UtcNow.AddSeconds(-2), Guid.NewGuid()),
                        new ProcedureImage(Guid.NewGuid(), "source", "channel", false, "path", "path", null, DateTimeOffset.UtcNow.AddSeconds(-1), Guid.NewGuid()),
                        new ProcedureImage(Guid.NewGuid(), "source", "channel", false, "path", "path", null, DateTimeOffset.UtcNow, Guid.NewGuid()),
                    },
                    Videos = new List<ProcedureVideo>(),
                    BackgroundVideos = new List<ProcedureVideo>(),
                    LibraryId = "libId",
                    RepositoryId = "repId",
                    ProcedureRelativePath = "path",
                    Department = null,
                    ProcedureType = new ProcedureType() { Id = 1, Name = "TestProceType" },
                    Physician = null,
                    RequiresUserConfirmation = false,
                    ProcedureStartTimeUtc = DateTimeOffset.UtcNow,
                    ProcedureTimezoneId = TimeZoneInfo.Local.Id,
                    IsClinical = false,
                    Notes = new List<ProcedureNote>(),
                    Accession = null,
                    RecordingEvents = new List<VideoRecordingEvent>(),
                    BackgroundRecordingMode = BackgroundRecordingMode.StartImmediately,
                    RegistrationMode = RegistrationMode.Quick,
                    PatientListSource = PatientListSource.Local
                });

        public static Faker<ActiveProcedureState> GetActiveProcedureWithCorrelationImagesFaker()
        {
            var correlationId = Guid.NewGuid();
            var activeProcedureFaker = new Faker<ActiveProcedureState>()
            .CustomInstantiator(f =>
                new ActiveProcedureState()
                {
                    Patient = new Patient() { LastName = "name" },
                    Images = new List<ProcedureImage>
                    {
                        new ProcedureImage(Guid.NewGuid(), "source", "channel", false, "path", "path", null, DateTimeOffset.UtcNow, correlationId),
                        new ProcedureImage(Guid.NewGuid(), "source", "channel", false, "path", "path", null, DateTimeOffset.UtcNow, correlationId),
                        new ProcedureImage(Guid.NewGuid(), "source", "channel", false, "path", "path", null, DateTimeOffset.UtcNow, correlationId),
                    },
                    Videos = new List<ProcedureVideo>(),
                    BackgroundVideos = new List<ProcedureVideo>(),
                    LibraryId = "libId",
                    RepositoryId = "repId",
                    ProcedureRelativePath = "path",
                    Department = null,
                    ProcedureType = new ProcedureType() { Id = 1, Name = "TestProceType" },
                    Physician = null,
                    RequiresUserConfirmation = false,
                    ProcedureStartTimeUtc = DateTimeOffset.UtcNow,
                    ProcedureTimezoneId = TimeZoneInfo.Local.Id,
                    IsClinical = false,
                    Notes = new List<ProcedureNote>(),
                    Accession = null,
                    RecordingEvents = new List<VideoRecordingEvent>(),
                    BackgroundRecordingMode = BackgroundRecordingMode.StartImmediately,
                    RegistrationMode = RegistrationMode.Quick,
                    PatientListSource = PatientListSource.Local
                }
            );

            return activeProcedureFaker;
        }

        public static Faker<AllocateNewProcedureResponse> GetAllocateNewProcedureResponseFaker() =>
            new Faker<AllocateNewProcedureResponse>()
                .CustomInstantiator(f =>
                    new AllocateNewProcedureResponse()
                    {
                        ProcedureId = new ProcedureIdMessage
                        {
                            Id = f.Random.String(24),
                            RepositoryName = f.Random.String(64)
                        },
                        RelativePath = f.System.FilePath()
                    }
                );
    }
}
