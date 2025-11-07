// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Brokers.DdsHttp;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Foundations.Patients;
using Moq;
using Tynamix.ObjectFiller;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Tests.Unit.Services.Patients
{
    public partial class PatientServiceTests
    {
        private readonly Mock<IDdsHttpBroker> ddsHttpBrokerMock;
        private readonly PatientService patientService;

        public PatientServiceTests()
        {
            this.ddsHttpBrokerMock = new Mock<IDdsHttpBroker>();

            this.patientService = new PatientService(
                ddsHttpBroker: this.ddsHttpBrokerMock.Object);
        }

        private static string GetRandomString() =>
            new MnemonicString().GetValue();

        private static Patient CreateRandomPatient()
        {
            var patient = new Patient();

            HumanName humanName = new HumanName
            {
                ElementId = GetRandomString(),
                Family = GetRandomString(),
                Given = new List<string> { GetRandomString() },
                Prefix = new List<string> { "Mr" },
                Use = HumanName.NameUse.Usual
            };

            patient.Name = new List<HumanName> { humanName };
            patient.Gender = AdministrativeGender.Male;
            patient.BirthDate = GetRandomString();

            return patient;
        }

        private Bundle CreateRandomBundle()
        {
            var bundle = new Bundle
            {
                Type = Bundle.BundleType.Searchset,
                Total = 1,
                Timestamp = DateTimeOffset.UtcNow
            };

            Patient patient = CreateRandomPatient();

            bundle.Entry = new List<Bundle.EntryComponent> {
                new Bundle.EntryComponent
                {
                    FullUrl = $"https://api.service.nhs.uk/personal-demographics/FHIR/STU3/Patient/{patient.Id}",
                    Search = new Bundle.SearchComponent { Score = 1 },
                    Resource = patient
                }
            };

            bundle.Meta = new Meta
            {
                LastUpdated = DateTimeOffset.UtcNow,
                Source = GetRandomString()
            };

            return bundle;
        }

        private string GetExpectedRequestBody(string nhsNumber, string dateOfBirth)
        {
            var parameters = new List<object>
            {
                new
                {
                    name = "patientNHSNumber",
                    valueIdentifier = new
                    {
                        system = "https://fhir.nhs.uk/Id/nhs-number",
                        value = nhsNumber
                    }
                },
                new
                {
                    name = "demographicsOnly",
                    part = new object[]
                    {
                        new
                        {
                            name = "includeDemographicsOnly",
                            valueBoolean = false
                        }
                    }
                },
                new
                {
                    name = "includeInactivePatients",
                    part = new object[]
                    {
                        new
                        {
                            name = "includeInactivePatients",
                            valueBoolean = false
                        }
                    }
                },
                new
                {
                    name = "patientDOB",
                    valueIdentifier = new
                    {
                        system = "https://fhir.hl7.org.uk/Id/dob",
                        value = dateOfBirth
                    }
                }
            };

            var requestBody = new
            {
                meta = new
                {
                    profile = new string[] {
                        "https://fhir.hl7.org.uk/STU3/OperationDefinition/CareConnect-GetStructuredRecord-Operation-1"
                    }
                },
                resourceType = "Parameters",
                parameter = parameters
            };

            return System.Text.Json.JsonSerializer.Serialize(requestBody);
        }
    }
}
