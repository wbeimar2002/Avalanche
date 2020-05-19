using Avalanche.Api.Mapping.Health;
using Avalanche.Api.ViewModels;
using Ism.Storage.Common.Core.PatientList.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalanche.Api.Tests.Mapping
{
    [TestFixture()]
    public class PieMappingTests
    {
        [Test]
        public void ExecutePatientSearchFieldsMapSucceeds()
        {
            PieMapping mapping = new PieMapping();

            var model = new PatientDetailsSearchFilterViewModel("Last", "Mrn", "accession", "department", DateTimeOffset.Now, DateTimeOffset.Now, "room", "procedure");
            var result = mapping.GetDetailsSearchFields(model);

            Assert.NotNull(result);
            Assert.AreEqual(model.AccessionNumber, result.AccessionNumber);
            Assert.AreEqual(model.DepartmentName, result.DepartmentName);
            Assert.AreEqual(model.LastName, result.LastName);
            Assert.AreEqual(model.MaxDate, result.MaxDate);
            Assert.AreEqual(model.MinDate, result.MinDate);
            Assert.AreEqual(model.MRN, result.MRN);
            Assert.AreEqual(model.ProcedureId, result.ProcedureId);
            Assert.AreEqual(model.RoomName, result.RoomName);
        }

        [Test]
        public void ExecutePatientRecordMapSucceeds()
        {
            PieMapping mapping = new PieMapping();

            var record = new PatientRecord
            {
                AccessionNumber = "accession",
                AdmissionStatus = Ism.Storage.Common.Core.PatientList.Enums.AdmissionStatus.Admit,
                Department = "department",
                Id = 5,
                MRN = "Mrn",
                Patient = new Patient("First", "Last", new NodaTime.LocalDate(2000, 2, 2), Ism.Storage.Common.Core.PatientList.Enums.Sex.F),
                PerformingPhysician = new Physician("physFirst", "physLast", "uid"),
                ProcedureType = "procType",
                Properties = new Dictionary<string, string> { { "prop1", "val1" } },
                Room = "Room",
                Scheduled = NodaTime.ZonedDateTime.FromDateTimeOffset(DateTimeOffset.Now),
                SecondaryPhysicians = new List<SecondaryPhysician>
                {
                    new SecondaryPhysician(new Physician("secFirst", "secLast", "secUid"), "secondary")
                }
            };

            var result = mapping.ToApiPatient(record);

            Assert.NotNull(result);
            Assert.AreEqual(record.AccessionNumber, result.AccessionNumber);
            Assert.AreEqual(record.Patient.Dob.ToDateTimeUnspecified().Date, result.DateOfBirth);
            Assert.AreEqual(record.Department, result.Department);
            Assert.AreEqual(record.Patient.Sex.ToString(), result.Gender);
            Assert.AreEqual(record.Id, result.Id);
            Assert.AreEqual(record.Patient.LastName, result.LastName); 
            Assert.AreEqual(record.MRN, result.MRN);
            Assert.AreEqual(record.Patient.FirstName, result.Name);
            Assert.AreEqual(record.Room, result.Room);
            Assert.AreEqual(record.ProcedureType, result.ProcedureType);
        }
    }
}
