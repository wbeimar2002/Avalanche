using System;
using System.Collections.Generic;
using System.Text;
using Bogus;
using Ism.SystemState.Models.Procedure;

namespace Avalanche.Api.Test
{
    public static class Fakers
    {
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

        public static Faker<ActiveProcedureState> GetActiveProcedureWhitImagesDifferentTimeFaker() =>
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

        public static Faker<ActiveProcedureState> GetActiveProcedureWhitCorrelationImagesFaker()
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
                });

            return activeProcedureFaker;
        }
    }
}
